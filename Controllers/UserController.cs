using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Do_an_co_so.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Trang cá nhân của người dùng (Chủ trọ)
        public async Task<IActionResult> TrangCaNhan(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // 1. Tìm thông tin chi tiết của chủ trọ
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // 2. Lấy tất cả phòng trọ do người này đăng
            var danhSachPhong = await _context.PhongTro
                .Where(p => p.ChuTroId == id)
                .ToListAsync();

            // Truyền thông tin Chủ trọ qua ViewBag để giao diện sử dụng
            ViewBag.ChuTro = user;

            // Truyền danh sách phòng vào Model
            return View(danhSachPhong);
        }
    }
}