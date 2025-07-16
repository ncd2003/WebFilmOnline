using WebFilmOnline.Services.ViewModels;

namespace WebFilmOnline.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseViewModel> CreatePaymentRequestAsync(int orderId, decimal amount, string orderDescription, string returnUrl, string? ipAddress = null);
        Task<bool> ProcessPaymentCallbackAsync(Dictionary<string, string> callbackData);
    }
}
