using WebFilmOnline.Models;
using WebFilmOnline.Services.ViewModels;

namespace WebFilmOnline.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetAllActiveProductsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<Product> AddProductAsync(Product product, int? createdByUserId);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> ProductExists(int productId);

        // Product Streaming Management
        Task<ProductStreaming?> GetProductStreamingByIdAsync(int productStreamingId);
        Task<ProductStreaming> AddProductStreamingAsync(ProductStreaming streamingInfo);
        Task<ProductStreaming> UpdateProductStreamingAsync(ProductStreaming streamingInfo);
        Task<bool> DeleteProductStreamingAsync(int productStreamingId);
        Task<IEnumerable<ProductStreaming>> GetStreamingSourcesForProductAsync(int productId);
    }
}
