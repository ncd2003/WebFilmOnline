using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebFilmOnline.Models;
using WebFilmOnline.Services.VNPay;
using WebFilmOnline.Services;

namespace WebFilmOnline.Controllers
{
    public class PointController : Controller
    {
        private readonly PointService _pointService;
        private readonly VnPayService _vnPayService;

        public PointController(PointService pointService, VnPayService vnPayService)
        {
            _pointService = pointService;
            _vnPayService = vnPayService;
        }

        // GET: /Point/Wallet
        // Hiển thị số dư ví và lịch sử giao dịch
        public async Task<IActionResult> Wallet()
        {
            // Lấy UserId của người dùng hiện tại
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem ví điểm.";
                return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
            }

            ViewBag.CurrentBalance = await _pointService.GetUserPointBalanceAsync(userId.Value);
            var transactions = await _pointService.GetUserPointTransactionsAsync(userId.Value);

            return View(transactions);
        }

        // GET: /Point/TopUp
        // Hiển thị form nạp tiền
        public IActionResult TopUp()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để nạp tiền.";
                return RedirectToAction("Login", "Account");
            }
            return View(new TopUpViewModel());
        }

        // POST: /Point/TopUp
        // Xử lý logic khởi tạo yêu cầu thanh toán VNPay
        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF
        public IActionResult TopUp(TopUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Tạo một mã giao dịch duy nhất cho riêng bạn
                // Ví dụ: kết hợp UserId và Timestamp
                string txnRef = $"TOPUP_{userId.Value}_{DateTime.Now.Ticks}";
                string orderInfo = $"Nap tien vao vi diem cho user {userId.Value}";
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Tạo URL thanh toán VNPay
                string paymentUrl = _vnPayService.CreatePaymentUrl(model.AmountVND, orderInfo, txnRef, ipAddress);

                // Lưu thông tin giao dịch tạm thời (TxnRef và UserId) vào DB để xác thực sau này
                // Đây là bước quan trọng để liên kết callback của VNPay với đúng người dùng.
                // Ví dụ: Bạn có thể tạo một bảng PaymentAttempt { Id, UserId, TxnRef, Amount, Status, CreatedAt }
                // Sau đó cập nhật Status trong callback.
                // Để đơn giản trong ví dụ này, tôi bỏ qua việc lưu vào DB.
                // Trong thực tế, BẮT BUỘC phải lưu để đảm bảo an toàn và tính toàn vẹn.

                return Redirect(paymentUrl); // Chuyển hướng người dùng đến trang VNPay
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating VNPay payment URL: {ex.Message}");
                ModelState.AddModelError("", "Không thể tạo yêu cầu thanh toán. Vui lòng thử lại.");
                return View(model);
            }
        }

        // GET: /Point/VnPayReturn
        // Xử lý callback/return từ VNPay
        [HttpGet]
        public async Task<IActionResult> VnPayReturn(VnPayCallbackViewModel model)
        {
            // Lấy tất cả các query parameters từ URL
            var queryCollection = HttpContext.Request.Query;

            // BƯỚC 1: Xác thực chữ ký của VNPay để đảm bảo dữ liệu không bị giả mạo
            bool isValidSignature = _vnPayService.ValidateSignature(model, queryCollection);

            if (!isValidSignature)
            {
                TempData["ErrorMessage"] = "Xác thực chữ ký không hợp lệ từ VNPay.";
                return RedirectToAction("Wallet");
            }

            // BƯỚC 2: Kiểm tra kết quả giao dịch từ VNPay
            // vnp_ResponseCode = "00" và vnp_TransactionStatus = "00" là thành công
            if (model.vnp_ResponseCode == "00" && model.vnp_TransactionStatus == "00")
            {
                // Giao dịch VNPay thành công
                // Lấy thông tin từ callback
                decimal amountVND = Convert.ToDecimal(model.vnp_Amount) / 100; // VNPay trả về số tiền nhân 100
                string txnRef = model.vnp_TxnRef;

                // GIẢ ĐỊNH: Lấy UserId từ txnRef.
                // TRONG THỰC TẾ: Bạn phải truy vấn DB để lấy UserId đã lưu cùng với txnRef khi khởi tạo giao dịch.
                // Ví dụ:
                // var paymentAttempt = await _context.PaymentAttempts.FirstOrDefaultAsync(pa => pa.TxnRef == txnRef);
                // if (paymentAttempt == null) { /* Xử lý lỗi: không tìm thấy giao dịch */ }
                // int userId = paymentAttempt.UserId;
                int userId = GetUserIdFromTxnRef(txnRef); // Hàm giả định

                if (userId == 0) // Kiểm tra userId hợp lệ
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng cho giao dịch này.";
                    return RedirectToAction("Wallet");
                }

                // BƯỚC 3: Cập nhật ví điểm của người dùng
                // Đảm bảo rằng giao dịch này chưa được xử lý trước đó (Idempotency)
                // Bạn có thể lưu TransactionNo hoặc TxnRef của VNPay vào PointTransaction
                // và kiểm tra trùng lặp trước khi thực hiện.
                // VD: bool transactionAlreadyProcessed = await _context.PointTransactions.AnyAsync(pt => pt.ReferenceId == model.vnp_TransactionNo);
                // If (!transactionAlreadyProcessed) { ... }

                bool success = await _pointService.TopUpPointsAsync(userId, amountVND, txnRef);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Nạp tiền thành công! Bạn đã nhận được {amountVND * PointService.ExchangeRateVNDToPoint} điểm.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ví điểm. Vui lòng liên hệ hỗ trợ.";
                }
            }
            else
            {
                // Giao dịch VNPay thất bại hoặc bị hủy
                TempData["ErrorMessage"] = $"Nạp tiền thất bại. Mã phản hồi: {model.vnp_ResponseCode} - Trạng thái: {model.vnp_TransactionStatus}.";
            }

            return RedirectToAction("Wallet");
        }

        // Phương thức helper để lấy UserId từ Claims (từ thông tin đăng nhập)
        private int? GetCurrentUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                // ClaimTypes.NameIdentifier thường lưu UserId khi bạn cấu hình xác thực
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return null;
        }

        // HÀM GIẢ ĐỊNH: Lấy UserId từ TxnRef (trong thực tế, hãy lấy từ DB)
        // TxnRef ví dụ có thể có dạng "TOPUP_123_TIMESTAMP"
        private int GetUserIdFromTxnRef(string txnRef)
        {
            if (string.IsNullOrEmpty(txnRef) || !txnRef.StartsWith("TOPUP_"))
            {
                return 0; // Hoặc ném ngoại lệ
            }
            var parts = txnRef.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out int userId))
            {
                return userId;
            }
            return 0; // Không tìm thấy hoặc lỗi định dạng
        }

    }
}
