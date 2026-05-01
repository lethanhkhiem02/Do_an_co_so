using Do_an_co_so.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Do_an_co_so.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<PhongTro> PhongTro { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.SoDu)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PhongTro>()
                .Property(p => p.Gia)
                .HasColumnType("decimal(18,2)");

        }
    }
}