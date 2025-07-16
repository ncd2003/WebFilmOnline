namespace WebFilmOnline.Models
{
    public class VnPayCallbackViewModel
    {
        // Các trường quan trọng từ VNPay callback, bạn có thể thêm/bớt tùy theo tài liệu VNPay
        public string vnp_Amount { get; set; }
        public string vnp_BankCode { get; set; }
        public string vnp_BankTranNo { get; set; }
        public string vnp_CardType { get; set; }
        public string vnp_OrderInfo { get; set; }
        public string vnp_PayDate { get; set; }
        public string vnp_ResponseCode { get; set; }
        public string vnp_TmnCode { get; set; }
        public string vnp_TransactionNo { get; set; }
        public string vnp_TransactionStatus { get; set; }
        public string vnp_TxnRef { get; set; } // Mã giao dịch của bạn
        public string vnp_SecureHash { get; set; }

        // Thêm các trường khác nếu cần
        public string vnp_SecureHashType { get; set; }
        public string vnp_TraceNo { get; set; }
        public string vnp_OrderType { get; set; }
        public string vnp_Locale { get; set; }
        public string vnp_Command { get; set; }
        public string vnp_IpAddr { get; set; }
    }
}
