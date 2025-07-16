using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;

namespace WebFilmOnline.Services.Implementations
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly FilmServiceDbContext _context;

        public ProductCategoryService(FilmServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync()
        {
            return await _context.ProductCategory.ToListAsync();
        }

        public async Task<ProductCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.ProductCategory.FindAsync(categoryId);
        }

        public async Task<ProductCategory> AddCategoryAsync(ProductCategory category)
        {
            if (await _context.ProductCategory.AnyAsync(c => c.Name == category.Name))
            {
                throw new InvalidOperationException($"Danh mục '{category.Name}' đã tồn tại.");
            }
            _context.ProductCategory.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<ProductCategory> UpdateCategoryAsync(ProductCategory category)
        {
            if (!await CategoryExists(category.CategoryId))
            {
                throw new ArgumentException("Danh mục không tồn tại.");
            }
            if (await _context.ProductCategory.AnyAsync(c => c.Name == category.Name && c.CategoryId != category.CategoryId))
            {
                throw new InvalidOperationException($"Danh mục '{category.Name}' đã tồn tại với ID khác.");
            }

            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.ProductCategory.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            // Kiểm tra xem có Product nào đang sử dụng danh mục này không
            if (await _context.Product.AnyAsync(p => p.CategoryId == categoryId))
            {
                throw new InvalidOperationException("Không thể xóa danh mục này vì có phim đang sử dụng nó.");
            }

            _context.ProductCategory.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CategoryExists(int categoryId)
        {
            return await _context.ProductCategory.AnyAsync(e => e.CategoryId == categoryId);
        }
    }
}