using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Data;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;
using WebFilmOnline.ViewModels;

namespace WebFilmOnline.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly FilmServiceDBContext _context;
        private readonly IPackageService _packageService;
        private readonly IProductService _productService;

        public OrderService(FilmServiceDBContext context, IPackageService packageService, IProductService productService)
        {
            _context = context;
            _packageService = packageService;
            _productService = productService;
        }

        public async Task<Order> CreatePackageOrderAsync(int userId, int packageId)
        {
            var package = await _packageService.GetPackageByIdAsync(packageId);
            if (package == null)
            {
                throw new ArgumentException("Gói dịch vụ không tồn tại.");
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = package.Price,
                PaymentStatus = "Pending"
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                PackageId = packageId,
                Quantity = 1,
                UnitPrice = package.Price,
                FinalPrice = package.Price
            };

            _context.OrderItem.Add(orderItem);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<Order> CreateProductOrderAsync(int userId, int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                throw new ArgumentException("Phim không tồn tại.");
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = product.Price,
                PaymentStatus = "Pending"
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                ProductId = productId,
                Quantity = 1,
                UnitPrice = product.Price,
                FinalPrice = product.Price
            };

            _context.OrderItem.Add(orderItem);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string paymentStatus, string? transactionId = null, string? paymentMethod = null)
        {
            var order = await _context.Order.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            order.PaymentStatus = paymentStatus;
            if (!string.IsNullOrEmpty(transactionId))
            {
                order.PaymentTransactionId = transactionId;
            }
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                order.PaymentMethod = paymentMethod;
            }

            _context.Order.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Order
                                 .Include(o => o.OrderItems)
                                     .ThenInclude(oi => oi.Package)
                                 .Include(o => o.OrderItems)
                                     .ThenInclude(oi => oi.Product)
                                 .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Order
                                 .Where(o => o.UserId == userId)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToListAsync();
        }
    }
}