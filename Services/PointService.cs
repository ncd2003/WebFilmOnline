using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Models;

namespace WebFilmOnline.Services
{
    public class PointService
    {
        private readonly FilmServiceDbContext _context;
        // Tỷ lệ quy đổi: 1 VNĐ = 1 Point (hoặc bất kỳ tỷ lệ nào bạn muốn)
        public const decimal ExchangeRateVNDToPoint = 1.0m;

        public PointService(FilmServiceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Nạp tiền vào ví điểm của người dùng sau khi giao dịch thanh toán thành công.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <param name="amountVND">Số tiền VNĐ đã nạp thành công.</param>
        /// <returns>True nếu nạp thành công, ngược lại False.</returns>
        public async Task<bool> TopUpPointsAsync(int userId, decimal amountVND, string transactionRef = null)
        {
            if (amountVND <= 0)
            {
                Console.WriteLine("Error: Số tiền nạp phải lớn hơn 0.");
                return false;
            }

            // Lấy hoặc tạo ví điểm cho người dùng
            var wallet = await _context.PointWallets
                                       .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new PointWallet
                {
                    UserId = userId,
                    Balance = 0, // Sẽ được cập nhật ngay sau đây
                    LastUpdated = DateTime.Now
                };
                _context.PointWallets.Add(wallet);
                // Không cần SaveChangesAsync ở đây nếu bạn muốn tất cả là một transaction lớn
            }

            decimal pointsEarned = amountVND * ExchangeRateVNDToPoint;

            // Cập nhật số dư ví
            wallet.Balance += pointsEarned;
            wallet.LastUpdated = DateTime.Now;

            // Ghi lại giao dịch nạp điểm
            var transaction = new PointTransaction
            {
                WalletId = wallet.WalletId,
                Amount = pointsEarned,
                Type = "Earn", // Loại giao dịch: Nạp điểm
                CreatedAt = DateTime.Now,
                RelatedOrderId = null // Trong trường hợp nạp tiền, thường không liên quan đến OrderId cụ thể
            };
            _context.PointTransactions.Add(transaction);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết hơn
                Console.WriteLine($"Error topping up points for UserId {userId}, Amount {amountVND}: {ex.Message}");
                // Log inner exception nếu có
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Lấy số dư ví điểm của người dùng.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <returns>Số dư điểm hiện tại, hoặc 0 nếu không có ví.</returns>
        public async Task<decimal> GetUserPointBalanceAsync(int userId)
        {
            var wallet = await _context.PointWallets
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(w => w.UserId == userId);
            return wallet?.Balance ?? 0;
        }

        /// <summary>
        /// Lấy lịch sử giao dịch điểm của người dùng.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <returns>Danh sách các giao dịch điểm.</returns>
        public async Task<IQueryable<PointTransaction>> GetUserPointTransactionsAsync(int userId)
        {
            var wallet = await _context.PointWallets
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                return Enumerable.Empty<PointTransaction>().AsQueryable();
            }

            // Tải các giao dịch của ví và sắp xếp theo thời gian mới nhất
            return _context.PointTransactions
                           .Where(pt => pt.WalletId == wallet.WalletId)
                           .OrderByDescending(pt => pt.CreatedAt)
                           .AsNoTracking();
        }

        // ... (Các phương thức khác như DeductPointsAsync nếu cần)
    }
}
