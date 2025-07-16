namespace WebFilmOnline.Models
{
    public class TransactionEvent
    {
        public string TransactionId { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string ProductType { get; set; } // Ví dụ: "Gói", "Phim lẻ", "Nạp Point"
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // Ví dụ: "VNPay", "Point"
        public string PromotionCode { get; set; }
        public string Status { get; set; } // Ví dụ: "Thành công", "Thất bại"
        public string Currency { get; set; } = "VND";

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] Giao dịch ID: {TransactionId}, SP: {ProductName}, Số tiền: {Amount} {Currency}, Trạng thái: {Status}";
        }
    }
}
