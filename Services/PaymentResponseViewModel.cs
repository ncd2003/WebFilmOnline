namespace WebFilmOnline.Services.ViewModels
{
    public class PaymentResponseViewModel
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? PaymentGatewayUrl { get; set; } // URL to redirect user to payment gateway
        public string? TransactionId { get; set; } // Internal transaction ID if applicable
    }
}