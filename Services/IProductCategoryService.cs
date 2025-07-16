using WebFilmOnline.Models;

namespace WebFilmOnline.Services.Interfaces
{
    public interface IProductCategoryService
    {
        Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync();
        Task<ProductCategory?> GetCategoryByIdAsync(int categoryId);
        Task<ProductCategory> AddCategoryAsync(ProductCategory category);
        Task<ProductCategory> UpdateCategoryAsync(ProductCategory category);
        Task<bool> DeleteCategoryAsync(int categoryId);
        Task<bool> CategoryExists(int categoryId);
    }
}
