using System.Net;
using System.Security.Cryptography;
using System.Text;
using WebFilmOnline.Models;

namespace WebFilmOnline.Services.VNPay
{
    public class VnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _baseUrl;
        private readonly string _returnUrl;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
            _tmnCode = _configuration["VnPayConfig:TmnCode"];
            _hashSecret = _configuration["VnPayConfig:HashSecret"];
            _baseUrl = _configuration["VnPayConfig:BaseUrl"];
            _returnUrl = _configuration["VnPayConfig:ReturnUrl"];
        }

        public string CreatePaymentUrl(decimal amount, string orderInfo, string txnRef, string ipAddress)
        {
            var vnp_Params = new SortedList<string, string>();
            vnp_Params.Add("vnp_Version", "2.1.0");
            vnp_Params.Add("vnp_Command", "pay");
            vnp_Params.Add("vnp_TmnCode", _tmnCode);
            vnp_Params.Add("vnp_Amount", (amount * 100).ToString()); // Số tiền VNPay yêu cầu nhân 100
            vnp_Params.Add("vnp_BankCode", ""); // Để trống nếu muốn người dùng chọn ngân hàng
            vnp_Params.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp_Params.Add("vnp_CurrCode", "VND");
            vnp_Params.Add("vnp_IpAddr", ipAddress);
            vnp_Params.Add("vnp_Locale", "vn");
            vnp_Params.Add("vnp_OrderInfo", orderInfo);
            vnp_Params.Add("vnp_OrderType", "other"); // Hoặc "billpayment", "goods", "topup"...
            vnp_Params.Add("vnp_ReturnUrl", _returnUrl);
            vnp_Params.Add("vnp_TxnRef", txnRef); // Mã giao dịch của bạn

            // Build query string
            StringBuilder query = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in vnp_Params)
            {
                if (!string.IsNullOrEmpty(entry.Value))
                {
                    query.AppendFormat("{0}={1}&", WebUtility.UrlEncode(entry.Key), WebUtility.UrlEncode(entry.Value));
                }
            }

            string data = query.ToString();
            string hashData = string.Join("&", vnp_Params.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{kv.Key}={kv.Value}"));
            string secureHash = HmacSHA512(_hashSecret, hashData);

            return $"{_baseUrl}?{data}vnp_SecureHash={secureHash}";
        }

        public bool ValidateSignature(VnPayCallbackViewModel model, IQueryCollection queryCollection)
        {
            var vnp_Params = new SortedList<string, string>();
            foreach (var s in queryCollection)
            {
                if (!string.IsNullOrEmpty(s.Value) && s.Key.StartsWith("vnp_"))
                {
                    vnp_Params.Add(s.Key, s.Value);
                }
            }

            var hashDigest = vnp_Params["vnp_SecureHash"];
            vnp_Params.Remove("vnp_SecureHash");
            vnp_Params.Remove("vnp_SecureHashType"); // Loại bỏ nếu có

            string data = string.Join("&", vnp_Params.Select(kv => $"{kv.Key}={kv.Value}"));
            string secureHash = HmacSHA512(_hashSecret, data);

            return secureHash == hashDigest;
        }

        private string HmacSHA512(string key, string input)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var b in hashValue)
                {
                    hash.AppendFormat("{0:x2}", b);
                }
            }
            return hash.ToString();
        }
    }
}
