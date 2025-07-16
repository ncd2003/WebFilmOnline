using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebFilmOnline.Services.Interfaces;
using WebFilmOnline.Models; // Để sử dụng Package model trong View

namespace WebFilmOnline.Controllers
{
    public class PackageController : Controller
    {
        private readonly IPackageService _packageService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public PackageController(
            IPackageService packageService,
            IOrderService orderService,
            IPaymentService paymentService)
        {
            _packageService = packageService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        // GET: /Package
        public async Task<IActionResult> Index()
        {
            var packages = await _packageService.GetAllActivePackagesAsync();
            return View(packages);
        }

        // GET: /Package/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            if (package == null)
            {
                return NotFound();
            }
            // Lấy danh sách các phim trong gói để hiển thị chi tiết
            ViewBag.ProductsInPackage = await _packageService.GetProductsInPackageAsync(id);
            return View(package);
        }

        [Authorize] // Chỉ người dùng đã đăng nhập mới có thể mua
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int packageId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mua gói dịch vụ.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Package", new { id = packageId }) });
            }

            var package = await _packageService.GetPackageByIdAsync(packageId);
            if (package == null)
            {
                TempData["ErrorMessage"] = "Gói dịch vụ không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Bước 1: Tạo đơn hàng
                var order = await _orderService.CreatePackageOrderAsync(userId, packageId);

                // Bước 2: Tạo yêu cầu thanh toán tới cổng thanh toán
                var returnUrl = Url.Action("PaymentCallback", "Payment", null, Request.Scheme); // URL mà cổng thanh toán sẽ gọi lại
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var paymentResponse = await _paymentService.CreatePaymentRequestAsync(
                    order.OrderId,
                    order.TotalAmount,
                    $"Thanh toan don hang {order.OrderId} mua goi '{package.Name}'",
                    returnUrl!, // Đảm bảo returnUrl không null
                    ipAddress
                );

                if (paymentResponse.IsSuccess && !string.IsNullOrEmpty(paymentResponse.PaymentGatewayUrl))
                {
                    // Chuyển hướng người dùng đến cổng thanh toán
                    return Redirect(paymentResponse.PaymentGatewayUrl);
                }
                else
                {
                    TempData["ErrorMessage"] = paymentResponse.Message ?? "Không thể tạo yêu cầu thanh toán. Vui lòng thử lại.";
                    // Cập nhật trạng thái đơn hàng nếu yêu cầu thanh toán thất bại
                    await _orderService.UpdateOrderStatusAsync(order.OrderId, "PaymentRequestFailed");
                    return RedirectToAction(nameof(Details), new { id = packageId });
                }
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = packageId });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để debug
                // _logger.LogError(ex, "Lỗi khi mua gói id {PackageId} cho người dùng id {UserId}", packageId, userId);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình mua gói dịch vụ. Vui lòng thử lại.";
                return RedirectToAction(nameof(Details), new { id = packageId });
            }
        }
    }
}
