namespace WebFilmOnline.Services.ViewModels
{
    public class PaymentRequestViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        // Add other properties needed by payment gateway (e.g., BankCode, Locale)
        public string? BankCode { get; set; }
        public string? IpAddress { get; set; } // IP of the user making the payment
    }
}