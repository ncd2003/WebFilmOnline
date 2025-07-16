using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Data;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;

namespace WebFilmOnline.Services.Implementations
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly FilmServiceDBContext _context;
        private readonly IPackageService _packageService;

        public UserSubscriptionService(FilmServiceDBContext context, IPackageService packageService)
        {
            _context = context;
            _packageService = packageService;
        }

        public async Task<UserSubscription> CreateOrUpdateSubscriptionAsync(int userId, int packageId)
        {
            var package = await _packageService.GetPackageByIdAsync(packageId);
            if (package == null)
            {
                throw new ArgumentException("Gói dịch vụ không tồn tại.");
            }

            var subscription = new UserSubscription
            {
                UserId = userId,
                PackageId = packageId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(package.DurationDays),
                RemainingCreditAmount = 0
            };

            _context.UserSubscription.Add(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }

        public async Task<IEnumerable<UserSubscription>> GetUserSubscriptionsAsync(int userId)
        {
            return await _context.UserSubscription
                                 .Include(us => us.Package)
                                 .Where(us => us.UserId == userId)
                                 .OrderByDescending(us => us.StartDate)
                                 .ToListAsync();
        }

        public async Task<bool> HasAccessToProductAsync(int userId, int productId)
        {
            // 1. Kiểm tra quyền truy cập thông qua gói đăng ký hiện có và còn hạn
            var hasSubscriptionAccess = await _context.UserSubscription
                                                    .AnyAsync(us => us.UserId == userId &&
                                                                    us.EndDate >= DateTime.Now &&
                                                                    _context.PackageProduct.Any(pp => pp.PackageId == us.PackageId && pp.ProductId == productId));
            if (hasSubscriptionAccess)
            {
                return true;
            }

            // 2. Kiểm tra quyền truy cập thông qua việc mua lẻ (Order và OrderItem)
            // Phim được mua lẻ nếu có một OrderItem với ProductId tương ứng
            // và Order liên quan có PaymentStatus là 'Paid'.
            var hasPurchasedDirectly = await _context.Order
                                                    .AnyAsync(o => o.UserId == userId &&
                                                                    o.PaymentStatus == "Paid" &&
                                                                    o.OrderItems.Any(oi => oi.ProductId == productId));
            if (hasPurchasedDirectly)
            {
                return true;
            }

            return false; // Không có quyền truy cập
        }

        // Triển khai cho mục 7 (nâng cấp gói) sẽ được thực hiện sau
        public Task<decimal> CalculateRemainingCreditForUpgradeAsync(int userId, int currentPackageId)
        {
            return Task.FromResult(0m);
        }

        public Task<UserSubscription> UpgradeSubscriptionAsync(int userId, int oldPackageId, int newPackageId, decimal remainingCreditUsed)
        {
            throw new NotImplementedException();
        }
    }
}