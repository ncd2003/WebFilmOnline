using WebFilmOnline.Models;

namespace WebFilmOnline.Services.Interfaces
{
    public interface IUserSubscriptionService
    {
        Task<UserSubscription> CreateOrUpdateSubscriptionAsync(int userId, int packageId);
        Task<IEnumerable<UserSubscription>> GetUserSubscriptionsAsync(int userId);
        Task<bool> HasAccessToProductAsync(int userId, int productId);
        Task<decimal> CalculateRemainingCreditForUpgradeAsync(int userId, int currentPackageId);
        Task<UserSubscription> UpgradeSubscriptionAsync(int userId, int oldPackageId, int newPackageId, decimal remainingCreditUsed);
    }
}