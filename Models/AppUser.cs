using Microsoft.AspNetCore.Identity;

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
    }
}