using System.Security.Claims;
using FilmServiceApp.Models; // Ensure this namespace matches your Models folder
using FilmServiceApp.Data; // Ensure this namespace matches your Data folder
using FilmServiceApp.Services.Interfaces; // Ensure this namespace matches your Interfaces folder
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // For logging

namespace FilmServiceApp.Services
{
    public class UserService : IUserService
    {
        private readonly FilmServiceDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(FilmServiceDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy hoặc tạo một người dùng dựa trên thông tin đăng nhập bên ngoài.
        /// Đây là logic cốt lõi để xử lý người dùng SSO.
        /// </summary>
        public async Task<User> GetOrCreateSSOUserAsync(IEnumerable<Claim> claims, string provider, string ssoId)
        {
            // Cố gắng tìm người dùng hiện có bằng nhà cung cấp SSO và ID
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.SSOProvider == provider && u.SSOId == ssoId);

            if (user == null)
            {
                // Người dùng không tồn tại với ID SSO này, tạo người dùng mới
                _logger.LogInformation($"Creating new user for SSO provider: {provider}, SSO ID: {ssoId}");

                // Trích xuất các claims cần thiết
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var fullName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                // Nếu email không được cung cấp bởi SSO, bạn có thể cần một phương án dự phòng hoặc yêu cầu người dùng nhập.
                // Hiện tại, chúng ta sẽ sử dụng một email mặc định hoặc ném lỗi nếu email là bắt buộc.
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning($"Email claim not found for SSO user from {provider}. SSO ID: {ssoId}");
                    // Bạn có thể muốn tạo một email duy nhất hoặc chuyển hướng đến một trang để yêu cầu email.
                    // Để đơn giản, chúng ta sẽ sử dụng một email được tạo.
                    email = $"{provider.ToLower()}_{ssoId}@sso.com"; // Email dự phòng
                }

                // Kiểm tra xem một người dùng với email này đã tồn tại chưa (ví dụ: họ đã đăng ký cục bộ)
                var existingUserByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (existingUserByEmail != null && existingUserByEmail.SSOProvider == null)
                {
                    // Nếu tìm thấy người dùng hiện có với cùng email và họ không có nhà cung cấp SSO,
                    // liên kết tài khoản SSO này với người dùng hiện có.
                    existingUserByEmail.SSOProvider = provider;
                    existingUserByEmail.SSOId = ssoId;
                    existingUserByEmail.FullName = existingUserByEmail.FullName ?? fullName; // Cập nhật tên đầy đủ nếu null
                    existingUserByEmail.Status = "Active"; // Đảm bảo trạng thái hoạt động
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Linked existing user (Email: {email}) with SSO provider: {provider}");
                    return existingUserByEmail;
                }
                else if (existingUserByEmail != null && existingUserByEmail.SSOProvider != null && existingUserByEmail.SSOId != ssoId)
                {
                    // Kịch bản này có nghĩa là email đã được liên kết với một nhà cung cấp SSO khác hoặc một ID SSO khác.
                    // Bạn có thể muốn xử lý bằng cách nhắc người dùng hoặc ngăn đăng nhập.
                    _logger.LogError($"Email {email} is already associated with a different SSO account ({existingUserByEmail.SSOProvider}). Cannot link.");
                    throw new InvalidOperationException($"Email {email} đã được liên kết với một tài khoản khác.");
                }

                // Tạo một thực thể User mới
                user = new User
                {
                    Email = email,
                    FullName = fullName,
                    SSOProvider = provider,
                    SSOId = ssoId,
                    CreatedAt = DateTime.Now,
                    Status = "Active"
                    // PasswordHash sẽ là null cho người dùng SSO
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"New user created successfully for {provider} with email: {email}");

                // Gán vai trò mặc định (ví dụ: 'Viewer') cho người dùng SSO mới
                var viewerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Viewer");
                if (viewerRole != null)
                {
                    _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = viewerRole.RoleId, AssignedAt = DateTime.Now });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Assigned 'Viewer' role to new user {user.UserId}.");
                }
                else
                {
                    _logger.LogWarning("'Viewer' role not found. Please ensure default roles are seeded in the database.");
                }
            }
            else
            {
                _logger.LogInformation($"Existing user found for SSO provider: {provider}, SSO ID: {ssoId}. User ID: {user.UserId}");
                // Tùy chọn, cập nhật chi tiết người dùng nếu chúng thay đổi ở phía nhà cung cấp SSO
                var currentFullName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                if (user.FullName != currentFullName)
                {
                    user.FullName = currentFullName;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Updated full name for user {user.UserId}.");
                }
            }

            return user;
        }

        public async Task<User?> FindUserBySSOAsync(string provider, string ssoId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.SSOProvider == provider && u.SSOId == ssoId);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
