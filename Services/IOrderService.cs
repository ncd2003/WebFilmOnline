using WebFilmOnline.Models;
using WebFilmOnline.Services.ViewModels;

namespace WebFilmOnline.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreatePackageOrderAsync(int userId, int packageId);
        Task<Order> CreateProductOrderAsync(int userId, int productId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string paymentStatus, string? transactionId = null, string? paymentMethod = null);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
    }
}
