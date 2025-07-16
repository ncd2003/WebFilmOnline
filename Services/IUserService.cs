using System.Security.Claims;
using FilmServiceApp.Models; // Đảm bảo namespace này khớp với thư mục Models của bạn

namespace FilmServiceApp.Services.Interfaces
{
    // Interface cho các hoạt động liên quan đến người dùng, đặc biệt là cho SSO
    public interface IUserService
    {
        /// <summary>
        /// Lấy hoặc tạo một người dùng dựa trên thông tin đăng nhập bên ngoài (SSO).
        /// </summary>
        /// <param name="claims">Các claims từ nhà cung cấp định danh bên ngoài (ví dụ: Google, Facebook).</param>
        /// <param name="provider">Tên của nhà cung cấp bên ngoài (ví dụ: "Google", "Facebook").</param>
        /// <param name="ssoId">ID duy nhất được cung cấp bởi nhà cung cấp bên ngoài.</param>
        /// <returns>Đối tượng User.</returns>
        Task<User> GetOrCreateSSOUserAsync(IEnumerable<Claim> claims, string provider, string ssoId);

        /// <summary>
        /// Tìm một người dùng bằng nhà cung cấp SSO và ID của họ.
        /// </summary>
        /// <param name="provider">Tên của nhà cung cấp bên ngoài.</param>
        /// <param name="ssoId">ID duy nhất từ nhà cung cấp bên ngoài.</param>
        /// <returns>Đối tượng User nếu tìm thấy, ngược lại là null.</returns>
        Task<User?> FindUserBySSOAsync(string provider, string ssoId);

        /// <summary>
        /// Tạo một người dùng mới trong cơ sở dữ liệu.
        /// </summary>
        /// <param name="user">Đối tượng User cần tạo.</param>
        /// <returns>Đối tượng User đã được tạo.</returns>
        Task<User> CreateUserAsync(User user);

        /// <summary>
        /// Cập nhật thông tin của một người dùng hiện có.
        /// </summary>
        /// <param name="user">Đối tượng User với thông tin đã cập nhật.</param>
        /// <returns>Đối tượng User đã được cập nhật.</returns>
        Task<User> UpdateUserAsync(User user);

        /// <summary>
        /// Lấy một người dùng bằng email của họ.
        /// </summary>
        /// <param name="email">Địa chỉ email của người dùng.</param>
        /// <returns>Đối tượng User nếu tìm thấy, ngược lại là null.</returns>
        Task<User?> GetUserByEmailAsync(string email);
    }
}
