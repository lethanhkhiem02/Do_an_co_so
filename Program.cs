using Do_an_co_so.Data;
using Do_an_co_so.Hubs;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
// ==========================================
// 1. CẤU HÌNH DATABASE VÀ IDENTITY
// ==========================================

// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký ApplicationDbContext với SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// TÔI ĐÃ COMMENT DÒNG NÀY LẠI VÌ NÓ GÂY LỖI TRÙNG LẶP (SCHEME ALREADY EXISTS)
// builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();


// Đăng ký Identity để quản lý User, Role và Phân quyền (Giữ lại đoạn code chuẩn này)
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    // Tùy chỉnh độ khó của mật khẩu (tắt các yêu cầu phức tạp để dễ test đồ án)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    // Không bắt buộc xác nhận email lúc mới làm đồ án cho dễ test
    options.SignIn.RequireConfirmedAccount = false;
})
.AddDefaultUI() // BẮT BUỘC PHẢI CÓ ĐỂ KẾT NỐI VỚI GIAO DIỆN IDENTITY (LOGIN/REGISTER)
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ==========================================

// Add services to the container.
builder.Services.AddControllersWithViews();

// THÊM DÒNG NÀY ĐỂ HỖ TRỢ RAZOR PAGES CỦA IDENTITY
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 2. MIDDLEWARE XÁC THỰC VÀ PHÂN QUYỀN
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// THÊM DÒNG NÀY ĐỂ ĐỊNH TUYẾN ĐƯỜNG DẪN CHO CÁC TRANG IDENTITY
app.MapRazorPages();

// ==========================================
// 2. TẠO TÀI KHOẢN ADMIN VÀ CÁC QUYỀN (ROLES) MẶC ĐỊNH
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // 1. Khởi tạo 3 nhóm quyền cho đồ án
    string[] roleNames = { "Admin", "ChuTro", "SinhVien" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 2. Tạo tài khoản Admin mặc định
    string adminEmail = "admin@gmail.com";
    string adminPass = "admin123"; // Mật khẩu đăng nhập của Admin

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            HoTen = "Quản Trị Viên Hệ Thống"
        };

        var result = await userManager.CreateAsync(adminUser, adminPass);
        if (result.Succeeded)
        {
            // Gán quyền Admin cho tài khoản vừa tạo
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
// ==========================================
app.MapHub<ChatHub>("/chatHub");
app.Run();