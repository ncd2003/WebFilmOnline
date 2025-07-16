using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebFilmOnline.Services.Interfaces;
using WebFilmOnline.Models; // Để sử dụng Product model trong View

namespace WebFilmOnline.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IUserSubscriptionService _userSubscriptionService;

        public ProductController(
            IProductService productService,
            IOrderService orderService,
            IPaymentService paymentService,
            IUserSubscriptionService userSubscriptionService)
        {
            _productService = productService;
            _orderService = orderService;
            _paymentService = paymentService;
            _userSubscriptionService = userSubscriptionService;
        }

        // GET: /Product
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllActiveProductsAsync();
            return View(products);
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            bool hasAccess = false;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                hasAccess = await _userSubscriptionService.HasAccessToProductAsync(userId, id);
            }
            ViewBag.HasAccess = hasAccess; // Đặt biến này để kiểm tra trong View
            ViewBag.CanPurchase = !hasAccess; // Chỉ cho phép mua nếu chưa sở hữu/đăng ký

            return View(product);
        }

        [Authorize] // Chỉ người dùng đã đăng nhập mới có thể mua
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int productId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mua phim.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Product", new { id = productId }) });
            }

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Phim không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra lại xem người dùng đã sở hữu phim này chưa (qua gói hoặc mua lẻ)
            if (await _userSubscriptionService.HasAccessToProductAsync(userId, productId))
            {
                TempData["ErrorMessage"] = "Bạn đã sở hữu phim này hoặc có quyền truy cập thông qua gói dịch vụ.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            try
            {
                // Bước 1: Tạo đơn hàng
                var order = await _orderService.CreateProductOrderAsync(userId, productId);

                // Bước 2: Tạo yêu cầu thanh toán tới cổng thanh toán
                var returnUrl = Url.Action("PaymentCallback", "Payment", null, Request.Scheme); // URL mà cổng thanh toán sẽ gọi lại
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var paymentResponse = await _paymentService.CreatePaymentRequestAsync(
                    order.OrderId,
                    order.TotalAmount,
                    $"Thanh toan don hang {order.OrderId} mua phim '{product.Title}'",
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
                    return RedirectToAction(nameof(Details), new { id = productId });
                }
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = productId });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để debug
                // _logger.LogError(ex, "Lỗi khi mua phim id {ProductId} cho người dùng id {UserId}", productId, userId);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình mua phim. Vui lòng thử lại.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }
        }
    }
}