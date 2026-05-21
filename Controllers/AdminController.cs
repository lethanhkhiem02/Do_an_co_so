using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Do_an_co_so.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = await _userManager.Users.Where(u => u.Id != currentUserId).ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng");

            user.TrangThaiKhoa = !user.TrangThaiKhoa;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng");

            // 1. Dọn dẹp Đánh giá
            var userReviews = await _context.DanhGias.Where(d => d.UserId == id).ToListAsync();
            _context.DanhGias.RemoveRange(userReviews);

            // 2. Dọn dẹp Tin nhắn
            var userMessages = await _context.Messages.Where(m => m.SenderId == id || m.ReceiverId == id).ToListAsync();
            _context.Messages.RemoveRange(userMessages);

            // 3. Dọn dẹp Hóa đơn
            var userInvoices = await _context.HoaDons.Where(h => h.NguoiThueId == id).ToListAsync();
            _context.HoaDons.RemoveRange(userInvoices);

            // 4. Nếu là chủ trọ: Dọn dẹp Phòng và các báo cáo liên quan đến phòng đó
            var userRooms = await _context.PhongTro.Where(p => p.ChuTroId == id).ToListAsync();
            foreach (var room in userRooms)
            {
                var roomReviews = await _context.DanhGias.Where(d => d.PhongTroId == room.Id).ToListAsync();
                _context.DanhGias.RemoveRange(roomReviews);

                var roomInvoices = await _context.HoaDons.Where(h => h.PhongTroId == room.Id).ToListAsync();
                _context.HoaDons.RemoveRange(roomInvoices);

                var roomReports = await _context.BaoCaos.Where(b => b.PhongTroId == room.Id).ToListAsync();
                _context.BaoCaos.RemoveRange(roomReports);
            }
            _context.PhongTro.RemoveRange(userRooms);

            // 5. Dọn dẹp các đơn báo cáo do người này GỬI
            var userReportsSent = await _context.BaoCaos.Where(b => b.NguoiBaoCaoId == id).ToListAsync();
            _context.BaoCaos.RemoveRange(userReportsSent);

            // 6. Dọn dẹp các đơn báo cáo mà người này BỊ TỐ CÁO
            var userReportsReceived = await _context.BaoCaos.Where(b => b.NguoiBiBaoCaoId == id).ToListAsync();
            _context.BaoCaos.RemoveRange(userReportsReceived);

            // Lưu toàn bộ quá trình dọn dẹp vào CSDL
            await _context.SaveChangesAsync();

            // 7. Xóa tài khoản
            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> QuanLyBaoCao()
        {
            var danhSachBaoCao = await _context.BaoCaos
                .Include(b => b.NguoiBaoCao)
                .Include(b => b.NguoiBiBaoCao)
                .Include(b => b.PhongTro)
                    .ThenInclude(p => p.ChuTro)
                .OrderByDescending(b => b.NgayBaoCao)
                .ToListAsync();

            return View(danhSachBaoCao);
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanDaXuLy(int id)
        {
            var report = await _context.BaoCaos.FindAsync(id);
            if (report != null)
            {
                report.DaXuLy = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("QuanLyBaoCao");
        }
    }
}