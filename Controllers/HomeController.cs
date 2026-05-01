using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Do_an_co_so.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // TÍNH NĂNG MỚI: Ưu tiên đẩy tin VIP lên đầu, sau đó mới đến tin mới nhất
            var danhSachPhong = await _context.PhongTro
                .OrderByDescending(p => p.IsVip)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            // Lấy toàn bộ dữ liệu đánh giá truyền ra giao diện
            ViewBag.AllDanhGias = await _context.DanhGias.ToListAsync();

            return View(danhSachPhong);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}