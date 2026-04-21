using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Thư viện để dùng biến IFormFile tải ảnh

namespace Do_an_co_so.Models
{
    public class PhongTro
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [Display(Name = "Mô tả phòng")]
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá phòng")]
        [Display(Name = "Giá thuê (VNĐ/Tháng)")]
        public decimal Gia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string DiaChi { get; set; }

        [Display(Name = "Hình ảnh minh họa")]
        public string? HinhAnh { get; set; } // Tên file ảnh sẽ lưu vào Database

        [NotMapped] // Khai báo này báo cho hệ thống biết KHÔNG tạo cột này trong CSDL
        [Display(Name = "Chọn các ảnh tải lên")]
        public List<IFormFile>? ImageUploads { get; set; } // Đã chuyển thành List để nhận nhiều ảnh

        // --- Liên kết để biết phòng này do Chủ Trọ nào đăng ---
        public string? ChuTroId { get; set; }
        [ForeignKey("ChuTroId")]
        public AppUser? ChuTro { get; set; }
    }
}