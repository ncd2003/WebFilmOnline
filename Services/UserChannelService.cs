using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;

namespace WebFilmOnline.Services.Implementations
{
    public class UserChannelService : IUserChannelService
    {
        private readonly FilmServiceDbContext _context;

        public UserChannelService(FilmServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserChannel>> GetAllUserChannelsAsync()
        {
            return await _context.UserChannels
                                 .Include(uc => uc.User)
                                 .Include(uc => uc.Provider)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<UserChannel>> GetUserChannelsByUserIdAsync(int userId)
        {
            return await _context.UserChannels
                                 .Include(uc => uc.Provider)
                                 .Where(uc => uc.UserId == userId)
                                 .ToListAsync();
        }

        public async Task<UserChannel?> GetUserChannelByIdAsync(int channelId)
        {
            return await _context.UserChannels
                                 .Include(uc => uc.User)
                                 .Include(uc => uc.Provider)
                                 .FirstOrDefaultAsync(uc => uc.ChannelId == channelId);
        }

        public async Task<UserChannel> AddUserChannelAsync(UserChannel channel)
        {
            if (!await _context.Users.AnyAsync(u => u.UserId == channel.UserId))
            {
                throw new ArgumentException("Người dùng không tồn tại.");
            }
            if (channel.ProviderId.HasValue && !await _context.StreamingProviders.AnyAsync(sp => sp.ProviderId == channel.ProviderId.Value))
            {
                throw new ArgumentException("Streaming Provider không tồn tại.");
            }

            _context.UserChannels.Add(channel);
            await _context.SaveChangesAsync();
            return channel;
        }

        public async Task<UserChannel> UpdateUserChannelAsync(UserChannel channel)
        {
            if (!await UserChannelExists(channel.ChannelId))
            {
                throw new ArgumentException("Kênh người dùng không tồn tại.");
            }
            if (!await _context.Users.AnyAsync(u => u.UserId == channel.UserId))
            {
                throw new ArgumentException("Người dùng không tồn tại.");
            }
            if (channel.ProviderId.HasValue && !await _context.StreamingProviders.AnyAsync(sp => sp.ProviderId == channel.ProviderId.Value))
            {
                throw new ArgumentException("Streaming Provider không tồn tại.");
            }

            _context.Entry(channel).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserChannelExists(channel.ChannelId))
                {
                    throw new ArgumentException("Kênh người dùng không tồn tại sau khi cập nhật.");
                }
                throw;
            }
            return channel;
        }

        public async Task<bool> DeleteUserChannelAsync(int channelId)
        {
            var channel = await _context.UserChannels.FindAsync(channelId);
            if (channel == null)
            {
                return false;
            }
            // Kiểm tra ràng buộc khóa ngoại (ProductStreaming)
            var hasProductStreamings = await _context.ProductStreamings.AnyAsync(ps => ps.ChannelId == channelId);
            if (hasProductStreamings)
            {
                throw new InvalidOperationException("Không thể xóa kênh này vì nó đang được sử dụng trong các nguồn streaming sản phẩm.");
            }

            _context.UserChannels.Remove(channel);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserChannelExists(int channelId)
        {
            return await _context.UserChannels.AnyAsync(e => e.ChannelId == channelId);
        }
    }
}