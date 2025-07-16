using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;

namespace WebFilmOnline.Services.Implementations
{
    public class StreamingProviderService : IStreamingProviderService
    {
        private readonly FilmServiceDbContext _context;

        public StreamingProviderService(FilmServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StreamingProvider>> GetAllProvidersAsync()
        {
            return await _context.StreamingProviders.ToListAsync();
        }

        public async Task<StreamingProvider?> GetProviderByIdAsync(int providerId)
        {
            return await _context.StreamingProviders.FindAsync(providerId);
        }

        public async Task<StreamingProvider> AddProviderAsync(StreamingProvider provider, int? createdByUserId)
        {
            if (await _context.StreamingProviders.AnyAsync(p => p.Name == provider.Name))
            {
                throw new InvalidOperationException("Tên nhà cung cấp đã tồn tại.");
            }
            provider.CreatedBy = createdByUserId;
            _context.StreamingProviders.Add(provider);
            await _context.SaveChangesAsync();
            return provider;
        }

        public async Task<StreamingProvider> UpdateProviderAsync(StreamingProvider provider)
        {
            if (!await ProviderExists(provider.ProviderId))
            {
                throw new ArgumentException("Nhà cung cấp không tồn tại.");
            }
            if (await _context.StreamingProviders.AnyAsync(p => p.Name == provider.Name && p.ProviderId != provider.ProviderId))
            {
                throw new InvalidOperationException("Tên nhà cung cấp đã tồn tại cho nhà cung cấp khác.");
            }

            _context.Entry(provider).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProviderExists(provider.ProviderId))
                {
                    throw new ArgumentException("Nhà cung cấp không tồn tại sau khi cập nhật.");
                }
                throw;
            }
            return provider;
        }

        public async Task<bool> DeleteProviderAsync(int providerId)
        {
            var provider = await _context.StreamingProviders.FindAsync(providerId);
            if (provider == null)
            {
                return false;
            }
            // Kiểm tra ràng buộc khóa ngoại (ProductStreaming, UserChannel)
            var hasProductStreamings = await _context.ProductStreamings.AnyAsync(ps => ps.ProviderId == providerId);
            var hasUserChannels = await _context.UserChannels.AnyAsync(uc => uc.ProviderId == providerId);

            if (hasProductStreamings || hasUserChannels)
            {
                throw new InvalidOperationException("Không thể xóa nhà cung cấp này vì nó đang được sử dụng trong các nguồn streaming hoặc kênh người dùng.");
            }

            _context.StreamingProviders.Remove(provider);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ProviderExists(int providerId)
        {
            return await _context.StreamingProviders.AnyAsync(e => e.ProviderId == providerId);
        }
    }
}