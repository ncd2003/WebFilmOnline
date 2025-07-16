using WebFilmOnline.Models;

namespace WebFilmOnline.Services.Interfaces
{
    public interface IUserChannelService
    {
        Task<IEnumerable<UserChannel>> GetAllUserChannelsAsync();
        Task<IEnumerable<UserChannel>> GetUserChannelsByUserIdAsync(int userId);
        Task<UserChannel?> GetUserChannelByIdAsync(int channelId);
        Task<UserChannel> AddUserChannelAsync(UserChannel channel);
        Task<UserChannel> UpdateUserChannelAsync(UserChannel channel);
        Task<bool> DeleteUserChannelAsync(int channelId);
        Task<bool> UserChannelExists(int channelId);
    }
}