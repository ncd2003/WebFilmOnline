using System.ComponentModel.DataAnnotations;

namespace WebFilmOnline.Models
{
    public class TopUpViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số tiền muốn nạp.")]
        [Range(10000, 100000000, ErrorMessage = "Số tiền nạp phải từ 10.000 VNĐ đến 100.000.000 VNĐ.")]
        [Display(Name = "Số tiền muốn nạp (VNĐ)")]
        public decimal AmountVND { get; set; }
    }
}
