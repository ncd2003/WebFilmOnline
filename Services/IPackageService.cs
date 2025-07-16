using WebFilmOnline.Models;
using WebFilmOnline.Services.ViewModels;

namespace WebFilmOnline.Services.Interfaces
{
    public interface IPackageService
    {
        Task<IEnumerable<Package>> GetAllPackagesAsync();
        Task<IEnumerable<Package>> GetAllActivePackagesAsync();
        Task<Package?> GetPackageByIdAsync(int packageId);
        Task<Package> AddPackageAsync(Package package, int? createdByUserId);
        Task<Package> UpdatePackageAsync(Package package);
        Task<bool> DeletePackageAsync(int packageId);
        Task<bool> PackageExists(int packageId);

        // Product within Package Management
        Task<IEnumerable<Product>> GetProductsInPackageAsync(int packageId);
        Task<bool> AddProductToPackageAsync(int packageId, int productId);
        Task<bool> RemoveProductFromPackageAsync(int packageId, int productId);
        Task<bool> IsProductInPackageAsync(int packageId, int productId);
    }
}