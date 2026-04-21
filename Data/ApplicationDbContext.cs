using Do_an_co_so.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Do_an_co_so.Data
{
    // Sử dụng IdentityDbContext thay vì DbContext bình thường
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Đã sửa 'PhongTros' thành 'PhongTro' để khớp với HomeController
        public DbSet<PhongTro> PhongTro { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<DanhGia> DanhGias { get; set; }
    }
}