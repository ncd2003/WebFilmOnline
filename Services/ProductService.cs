using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Data;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;
using WebFilmOnline.Services.ViewModels; // Thêm nếu dùng ViewModel

namespace WebFilmOnline.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly FilmServiceDBContext _context;

        public ProductService(FilmServiceDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Product
                                 .Include(p => p.Category)
                                 .OrderBy(p => p.Title)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllActiveProductsAsync()
        {
            return await _context.Product
                                 .Include(p => p.Category)
                                 .Where(p => p.IsActive)
                                 .OrderBy(p => p.Title)
                                 .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Product
                                 .Include(p => p.Category)
                                 .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<Product> AddProductAsync(Product product, int? createdByUserId)
        {
            // Kiểm tra CategoryId có tồn tại không
            if (!await _context.ProductCategory.AnyAsync(c => c.CategoryId == product.CategoryId))
            {
                throw new ArgumentException("Danh mục không tồn tại.");
            }
            // Optional: Check if product with same title exists in same category
            if (await _context.Product.AnyAsync(p => p.Title == product.Title && p.CategoryId == product.CategoryId))
            {
                throw new InvalidOperationException($"Phim '{product.Title}' đã tồn tại trong danh mục này.");
            }

            product.CreatedBy = createdByUserId; // Gán người tạo nếu có
            _context.Product.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            if (!await ProductExists(product.ProductId))
            {
                throw new ArgumentException("Phim không tồn tại.");
            }
            if (!await _context.ProductCategory.AnyAsync(c => c.CategoryId == product.CategoryId))
            {
                throw new ArgumentException("Danh mục không tồn tại.");
            }
            // Optional: Check for duplicate title in same category
            if (await _context.Product.AnyAsync(p => p.Title == product.Title && p.CategoryId == product.CategoryId && p.ProductId != product.ProductId))
            {
                throw new InvalidOperationException($"Phim '{product.Title}' đã tồn tại trong danh mục này.");
            }

            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(product.ProductId))
                {
                    throw new ArgumentException("Phim không tồn tại sau khi cập nhật.");
                }
                throw;
            }
            return product;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return false;
            }
            // Kiểm tra ràng buộc khóa ngoại (PackageProduct, ProductStreaming, OrderItem)
            var hasPackageProducts = await _context.PackageProduct.AnyAsync(pp => pp.ProductId == productId);
            var hasProductStreamings = await _context.ProductStreaming.AnyAsync(ps => ps.ProductId == productId);
            var hasOrderItems = await _context.OrderItem.AnyAsync(oi => oi.ProductId == productId);

            if (hasPackageProducts || hasProductStreamings || hasOrderItems)
            {
                throw new InvalidOperationException("Không thể xóa phim này vì nó đang được sử dụng trong các gói, nguồn streaming hoặc đơn hàng.");
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ProductExists(int productId)
        {
            return await _context.Product.AnyAsync(e => e.ProductId == productId);
        }

        // Product Streaming Management
        public async Task<IEnumerable<ProductStreaming>> GetStreamingSourcesForProductAsync(int productId)
        {
            return await _context.ProductStreaming
                                 .Include(ps => ps.Provider)
                                 .Include(ps => ps.Channel)
                                     .ThenInclude(ch => ch.User) // Bao gồm User của Channel nếu cần
                                 .Where(ps => ps.ProductId == productId)
                                 .OrderBy(ps => ps.Priority)
                                 .ToListAsync();
        }

        public async Task<ProductStreaming?> GetProductStreamingByIdAsync(int productStreamingId)
        {
            return await _context.ProductStreaming
                                 .Include(ps => ps.Provider)
                                 .Include(ps => ps.Channel)
                                 .FirstOrDefaultAsync(ps => ps.ProductStreamingId == productStreamingId);
        }

        public async Task<ProductStreaming> AddProductStreamingAsync(ProductStreaming streamingInfo)
        {
            if (!await ProductExists(streamingInfo.ProductId))
            {
                throw new ArgumentException("Sản phẩm không tồn tại.");
            }
            // Validate ProviderId XOR ChannelId
            if (streamingInfo.ProviderId == null && streamingInfo.ChannelId == null)
            {
                throw new ArgumentException("Phải chỉ định Provider hoặc Channel.");
            }
            if (streamingInfo.ProviderId != null && streamingInfo.ChannelId != null)
            {
                throw new ArgumentException("Không thể chỉ định cả Provider và Channel. Chỉ chọn một.");
            }
            if (streamingInfo.ProviderId.HasValue && !await _context.StreamingProvider.AnyAsync(sp => sp.ProviderId == streamingInfo.ProviderId.Value))
            {
                throw new ArgumentException("Streaming Provider không tồn tại.");
            }
            if (streamingInfo.ChannelId.HasValue && !await _context.UserChannel.AnyAsync(uc => uc.ChannelId == streamingInfo.ChannelId.Value))
            {
                throw new ArgumentException("User Channel không tồn tại.");
            }

            _context.ProductStreaming.Add(streamingInfo);
            await _context.SaveChangesAsync();
            return streamingInfo;
        }

        public async Task<ProductStreaming> UpdateProductStreamingAsync(ProductStreaming streamingInfo)
        {
            if (!await _context.ProductStreaming.AnyAsync(ps => ps.ProductStreamingId == streamingInfo.ProductStreamingId))
            {
                throw new ArgumentException("Thông tin streaming không tồn tại.");
            }
            if (!await ProductExists(streamingInfo.ProductId))
            {
                throw new ArgumentException("Sản phẩm không tồn tại.");
            }
            // Validate ProviderId XOR ChannelId
            if (streamingInfo.ProviderId == null && streamingInfo.ChannelId == null)
            {
                throw new ArgumentException("Phải chỉ định Provider hoặc Channel.");
            }
            if (streamingInfo.ProviderId != null && streamingInfo.ChannelId != null)
            {
                throw new ArgumentException("Không thể chỉ định cả Provider và Channel. Chỉ chọn một.");
            }
            if (streamingInfo.ProviderId.HasValue && !await _context.StreamingProvider.AnyAsync(sp => sp.ProviderId == streamingInfo.ProviderId.Value))
            {
                throw new ArgumentException("Streaming Provider không tồn tại.");
            }
            if (streamingInfo.ChannelId.HasValue && !await _context.UserChannel.AnyAsync(uc => uc.ChannelId == streamingInfo.ChannelId.Value))
            {
                throw new ArgumentException("User Channel không tồn tại.");
            }

            _context.Entry(streamingInfo).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ProductStreaming.AnyAsync(ps => ps.ProductStreamingId == streamingInfo.ProductStreamingId))
                {
                    throw new ArgumentException("Thông tin streaming không tồn tại sau khi cập nhật.");
                }
                throw;
            }
            return streamingInfo;
        }

        public async Task<bool> DeleteProductStreamingAsync(int productStreamingId)
        {
            var streamingInfo = await _context.ProductStreaming.FindAsync(productStreamingId);
            if (streamingInfo == null)
            {
                return false;
            }
            _context.ProductStreaming.Remove(streamingInfo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}