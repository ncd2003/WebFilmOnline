using WebFilmOnline.Models;

public class PaymentService
{
    public bool ProcessPayment(int userId, double amount)
    {
        // Giả lập thanh toán thành công
        // (sau này có thể tích hợp VNPay, Momo...)

        // Nếu bạn muốn trừ từ PointWallet:
        // var wallet = context.PointWallets.FirstOrDefault(w => w.UserId == userId);
        // if (wallet.Points < amount) return false;
        // wallet.Points -= (int)amount;
        // context.SaveChanges();

        return true;
    }
}
