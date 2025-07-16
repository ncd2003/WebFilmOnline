using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FilmServiceApp.Services.Interfaces; // Ensure this namespace matches your Interfaces folder
using FilmServiceApp.Data; // Ensure this namespace matches your Data folder
using Microsoft.EntityFrameworkCore; // Required for .Include() and .ToListAsync()
using Microsoft.Extensions.Logging; // For ILogger

namespace FilmServiceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly FilmServiceDbContext _context; // Inject DbContext to access roles directly for claims
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, FilmServiceDbContext context, ILogger<AccountController> logger)
        {
            _userService = userService;
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/ExternalLogin
        /// <summary>
        /// Bắt đầu quá trình đăng nhập bên ngoài (ví dụ: với Google hoặc Facebook).
        /// </summary>
        /// <param name="provider">Tên của nhà cung cấp bên ngoài (ví dụ: "Google", "Facebook").</param>
        /// <param name="returnUrl">URL để chuyển hướng đến sau khi đăng nhập thành công.</param>
        /// <returns>Một ChallengeResult để chuyển hướng đến trang đăng nhập của nhà cung cấp bên ngoài.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo vệ chống lại Cross-Site Request Forgery
        public IActionResult ExternalLogin(string provider, string returnUrl = "/")
        {
            // Yêu cầu chuyển hướng đến nhà cung cấp đăng nhập bên ngoài
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        // GET: /Account/ExternalLoginCallback
        /// <summary>
        /// Xử lý callback từ nhà cung cấp đăng nhập bên ngoài sau khi xác thực thành công.
        /// </summary>
        /// <param name="returnUrl">URL để chuyển hướng đến sau khi đăng nhập thành công.</param>
        /// <param name="remoteError">Thông báo lỗi từ nhà cung cấp bên ngoài, nếu có.</param>
        /// <returns>Chuyển hướng đến URL ban đầu hoặc trang đăng nhập với lỗi.</returns>
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError($"Error from external provider: {remoteError}");
                TempData["ErrorMessage"] = $"Lỗi từ nhà cung cấp đăng nhập: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            // Đọc thông tin về đăng nhập bên ngoài
            var info = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (info?.Principal == null)
            {
                _logger.LogWarning("No principal found after external authentication.");
                TempData["ErrorMessage"] = "Không thể lấy thông tin đăng nhập từ nhà cung cấp bên ngoài.";
                return RedirectToAction(nameof(Login));
            }

            // Lấy tên của nhà cung cấp bên ngoài (ví dụ: "Google", "Facebook")
            var externalProvider = info.Principal.Identity?.AuthenticationType;
            if (string.IsNullOrEmpty(externalProvider))
            {
                _logger.LogWarning("External provider name is null or empty.");
                TempData["ErrorMessage"] = "Không xác định được nhà cung cấp đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            // Tìm định danh duy nhất cho người dùng từ nhà cung cấp bên ngoài
            var ssoIdClaim = info.Principal.FindFirst(ClaimTypes.NameIdentifier);
            if (ssoIdClaim == null)
            {
                _logger.LogWarning($"NameIdentifier claim not found for external provider: {externalProvider}");
                TempData["ErrorMessage"] = "Không tìm thấy định danh người dùng từ nhà cung cấp bên ngoài.";
                return RedirectToAction(nameof(Login));
            }
            var ssoId = ssoIdClaim.Value;

            try
            {
                // Sử dụng UserService để lấy hoặc tạo người dùng dựa trên thông tin SSO
                var user = await _userService.GetOrCreateSSOUserAsync(info.Principal.Claims, externalProvider, ssoId);

                // Tạo claims cho xác thực nội bộ của ứng dụng
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // UserId nội bộ
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                    new Claim("SSOProvider", user.SSOProvider ?? "Local"), // Lưu nhà cung cấp SSO
                    new Claim("SSOId", user.SSOId ?? "") // Lưu ID SSO
                    // Thêm các claims khác nếu cần, ví dụ: vai trò
                };

                // Thêm vai trò vào claims
                var userRoles = await _context.UserRoles.Where(ur => ur.UserId == user.UserId)
                                                        .Include(ur => ur.Role)
                                                        .Select(ur => ur.Role.Name)
                                                        .ToListAsync();
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Giữ người dùng đăng nhập qua các phiên duyệt web
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // Đặt thời gian hết hạn cho cookie
                };

                // Đăng nhập người dùng nội bộ với identity claims đã tạo
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                _logger.LogInformation($"User {user.Email} logged in successfully via {externalProvider}.");

                // Chuyển hướng đến URL ban đầu hoặc trang chủ
                return LocalRedirect(returnUrl);
            }
            catch (InvalidOperationException ex)
            {
                // Xử lý các lỗi cụ thể, ví dụ: email đã được liên kết với một SSO khác
                _logger.LogError(ex, $"SSO login error: {ex.Message}");
                TempData["ErrorMessage"] = ex.Message; // Hiển thị thông báo lỗi cho người dùng
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during external login callback.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi không mong muốn trong quá trình đăng nhập. Vui lòng thử lại.";
                return RedirectToAction(nameof(Login));
            }
        }

        // POST: /Account/Logout
        /// <summary>
        /// Đăng xuất người dùng hiện tại.
        /// </summary>
        /// <returns>Chuyển hướng đến trang chủ sau khi đăng xuất.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Đăng xuất người dùng khỏi scheme xác thực cookie của ứng dụng
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chủ
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
