using WebFilmOnline.Models;

namespace WebFilmOnline.Services.Interfaces
{
    public interface IStreamingProviderService
    {
        Task<IEnumerable<StreamingProvider>> GetAllProvidersAsync();
        Task<StreamingProvider?> GetProviderByIdAsync(int providerId);
        Task<StreamingProvider> AddProviderAsync(StreamingProvider provider, int? createdByUserId);
        Task<StreamingProvider> UpdateProviderAsync(StreamingProvider provider);
        Task<bool> DeleteProviderAsync(int providerId);
        Task<bool> ProviderExists(int providerId);
    }
}
