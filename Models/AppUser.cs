using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Do_an_co_so.Models
{
    // Kế thừa IdentityUser để có sẵn các cột: Id, Email, PasswordHash, PhoneNumber...
    public class AppUser : IdentityUser
    {
        public string HoTen { get; set; }
        // Sau này bạn có thể thêm: DiaChi, NgaySinh, v.v. ở đây

        public string? Avatar { get; set; }       // Lưu tên file ảnh đại diện
        public DateTime? NgaySinh { get; set; }   // Lưu ngày tháng năm sinh
        public string? DiaChi { get; set; }       // Lưu địa chỉ

        // --- CÁC THUỘC TÍNH MỚI CHO TÍNH NĂNG VIP ---
        [Display(Name = "Số dư ví")]
        public decimal SoDu { get; set; } = 0; // Mặc định ví tạo ra có 0 đồng

        // --- THUỘC TÍNH MỚI DÀNH CHO ADMIN ---
        [Display(Name = "Trạng thái khóa")]
        public bool TrangThaiKhoa { get; set; } = false; // Mặc định tài khoản mới tạo không bị khóa
    }
}