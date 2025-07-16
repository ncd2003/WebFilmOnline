using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;
using WebFilmOnline.Services.ViewModels; // Thêm nếu dùng ViewModel

namespace WebFilmOnline.Services.Implementations
{
    public class PackageService : IPackageService
    {
        private readonly FilmServiceDbContext _context;

        public PackageService(FilmServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Package>> GetAllPackagesAsync()
        {
            return await _context.Packages
                                 .OrderBy(p => p.Name)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Package>> GetAllActivePackagesAsync()
        {
            return await _context.Packages
                                 .Where(p => (bool)p.IsActive)
                                 .OrderBy(p => p.Name)
                                 .ToListAsync();
        }

        public async Task<Package?> GetPackageByIdAsync(int packageId)
        {
            return await _context.Packages.FindAsync(packageId);
        }

        public async Task<Package> AddPackageAsync(Package package, int? createdByUserId)
        {
            if (await _context.Packages.AnyAsync(p => p.Name == package.Name))
            {
                throw new InvalidOperationException("Tên gói đã tồn tại.");
            }
            package.CreatedBy = createdByUserId; // Gán người tạo nếu có
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<Package> UpdatePackageAsync(Package package)
        {
            if (!await PackageExists(package.PackageId))
            {
                throw new ArgumentException("Gói không tồn tại.");
            }
            if (await _context.Packages.AnyAsync(p => p.Name == package.Name && p.PackageId != package.PackageId))
            {
                throw new InvalidOperationException("Tên gói đã tồn tại cho gói khác.");
            }

            _context.Entry(package).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PackageExists(package.PackageId))
                {
                    throw new ArgumentException("Gói không tồn tại sau khi cập nhật.");
                }
                throw;
            }
            return package;
        }

        public async Task<bool> DeletePackageAsync(int packageId)
        {
            var package = await _context.Packages.FindAsync(packageId);
            if (package == null)
            {
                return false;
            }
            // Kiểm tra ràng buộc khóa ngoại (PackageProduct, OrderItem, UserSubscription)
            var hasPackageProducts = await _context.Packages.AnyAsync(pp => pp.PackageId == packageId);
            var hasOrderItems = await _context.OrderItems.AnyAsync(oi => oi.PackageId == packageId);
            var hasUserSubscriptions = await _context.UserSubscriptions.AnyAsync(us => us.PackageId == packageId);

            if (hasPackageProducts || hasOrderItems || hasUserSubscriptions)
            {
                throw new InvalidOperationException("Không thể xóa gói này vì nó đang chứa sản phẩm, trong các đơn hàng hoặc có người dùng đang đăng ký.");
            }

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PackageExists(int packageId)
        {
            return await _context.Packages.AnyAsync(e => e.PackageId == packageId);
        }

        // Product within Package Management
        public async Task<IEnumerable<Product>> GetProductsInPackageAsync(int packageId)
        {
            return await _context.Packages
                                 .Where(pp => pp.PackageId == packageId)
                                 .Select(pp => pp.Product)
                                 .ToListAsync();
        }

        public async Task<bool> AddProductToPackageAsync(int packageId, int productId)
        {
            if (!await PackageExists(packageId))
            {
                throw new ArgumentException("Gói không tồn tại.");
            }
            if (!await _context.Product.AnyAsync(p => p.ProductId == productId))
            {
                throw new ArgumentException("Phim không tồn tại.");
            }
            if (await IsProductInPackageAsync(packageId, productId))
            {
                return false; // Đã tồn tại
            }

            _context.PackageProduct.Add(new PackageProduct { PackageId = packageId, ProductId = productId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveProductFromPackageAsync(int packageId, int productId)
        {
            var packageProduct = await _context.PackageProduct
                                               .FirstOrDefaultAsync(pp => pp.PackageId == packageId && pp.ProductId == productId);
            if (packageProduct == null)
            {
                return false; // Không tìm thấy
            }

            _context.PackageProduct.Remove(packageProduct);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsProductInPackageAsync(int packageId, int productId)
        {
            return await _context.PackageProduct.AnyAsync(pp => pp.PackageId == packageId && pp.ProductId == productId);
        }
    }
}
