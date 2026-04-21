using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_an_co_so.Models
{
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }

        // Đánh giá cho phòng trọ nào?
        public int PhongTroId { get; set; }
        [ForeignKey("PhongTroId")]
        public PhongTro? PhongTro { get; set; }

        // Ai là người đánh giá? (Sinh viên)
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao")]
        [Range(1, 5)]
        public int Sao { get; set; } // Lưu số sao từ 1 đến 5

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        public string NoiDung { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now; // Tự động lấy giờ hiện tại
    }
}