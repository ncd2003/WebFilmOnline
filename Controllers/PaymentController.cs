using Microsoft.AspNetCore.Mvc;
using WebFilmOnline.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Để ghi log lỗi

namespace WebFilmOnline.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger; // Để ghi log lỗi

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // Đây là endpoint mà cổng thanh toán sẽ gọi lại sau khi giao dịch hoàn tất
        // Tên Action và tham số có thể thay đổi tùy thuộc vào tài liệu của cổng thanh toán (ví dụ: VNPay)
        [HttpGet]
        [HttpPost] // Cổng thanh toán có thể gửi GET hoặc POST
        public async Task<IActionResult> PaymentCallback()
        {
            // Đọc dữ liệu từ query string (thường dùng cho GET callback)
            Dictionary<string, string> callbackData = new Dictionary<string, string>();
            foreach (string key in Request.Query.Keys)
            {
                callbackData[key] = Request.Query[key];
            }
            // Nếu cổng thanh toán gửi POST, bạn sẽ cần đọc từ Request.Form hoặc Request.Body.
            // Ví dụ: foreach (string key in Request.Form.Keys) { callbackData[key] = Request.Form[key]; }

            _logger.LogInformation($"PaymentCallback received: {string.Join(", ", callbackData.Select(kv => $"{kv.Key}={kv.Value}"))}");

            if (callbackData.TryGetValue("vnp_ResponseCode", out string? responseCode))
            {
                if (responseCode == "00") // Giao dịch thành công (mã của VNPay)
                {
                    bool success = await _paymentService.ProcessPaymentCallbackAsync(callbackData);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Thanh toán thành công! Dịch vụ/phim đã được kích hoạt.";
                        // Lấy OrderId từ callback để chuyển hướng đến trang chi tiết đơn hàng
                        if (callbackData.TryGetValue("vnp_TxnRef", out string? orderIdStr) && int.TryParse(orderIdStr, out int orderId))
                        {
                            return RedirectToAction("Details", "Order", new { id = orderId }); // Giả định có OrderController và Details action
                        }
                        return RedirectToAction("PurchaseSuccess", "Payment");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Thanh toán thành công nhưng có lỗi trong quá trình kích hoạt dịch vụ/phim. Vui lòng liên hệ hỗ trợ.";
                        _logger.LogError("PaymentCallback: Error processing payment for order from callback. Data: {callbackData}", string.Join(", ", callbackData.Select(kv => $"{kv.Key}={kv.Value}")));
                        return RedirectToAction("PurchaseFailure", "Payment");
                    }
                }
                else // Giao dịch thất bại hoặc bị hủy (mã khác 00 của VNPay)
                {
                    TempData["ErrorMessage"] = $"Thanh toán thất bại hoặc bị hủy. Mã lỗi: {responseCode}";
                    // Vẫn gọi ProcessPaymentCallbackAsync để cập nhật trạng thái đơn hàng là 'Failed'
                    await _paymentService.ProcessPaymentCallbackAsync(callbackData);
                    return RedirectToAction("PurchaseFailure", "Payment");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu callback không hợp lệ.";
                _logger.LogError("PaymentCallback: Invalid callback data. Missing vnp_ResponseCode. Data: {callbackData}", string.Join(", ", callbackData.Select(kv => $"{kv.Key}={kv.Value}")));
                return RedirectToAction("PurchaseFailure", "Payment");
            }
        }

        // Trang báo cáo thanh toán thành công
        public IActionResult PurchaseSuccess()
        {
            return View();
        }

        // Trang báo cáo thanh toán thất bại
        public IActionResult PurchaseFailure()
        {
            return View();
        }

        // Optional: User's Order History and Details (can be in a separate OrderController)
        // For brevity, I'm just listing the methods. You might create a dedicated OrderController.
        /*
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account"); // Or handle error
            }
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.UserId != userId)
            {
                return NotFound(); // Or AccessDenied
            }
            return View(order);
        }
        */
    }
}
