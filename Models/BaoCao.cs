using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_an_co_so.Models
{
    public class BaoCao
    {
        [Key]
        public int Id { get; set; }

        // Người GỬI báo cáo (Ai là người viết đơn)
        public string NguoiBaoCaoId { get; set; }
        [ForeignKey("NguoiBaoCaoId")]
        public AppUser NguoiBaoCao { get; set; }

        // --- MỚI: Người BỊ báo cáo (Dành cho tố cáo xúc phạm nhau) ---
        public string? NguoiBiBaoCaoId { get; set; }
        [ForeignKey("NguoiBiBaoCaoId")]
        public AppUser? NguoiBiBaoCao { get; set; }

        // Bài đăng phòng trọ bị tố cáo (Đã thêm dấu ? để cho phép bỏ trống nếu chỉ tố cáo người)
        public int? PhongTroId { get; set; }
        [ForeignKey("PhongTroId")]
        public PhongTro? PhongTro { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lý do báo cáo")]
        public string LyDo { get; set; }

        public string ChiTiet { get; set; }

        public DateTime NgayBaoCao { get; set; } = DateTime.Now;

        public bool DaXuLy { get; set; } = false;
    }
}