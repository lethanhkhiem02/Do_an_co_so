using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Do_an_co_so.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào các chức năng trong này
    public class PhongTroController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUser> _userManager;

        public PhongTroController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [Authorize(Roles = "ChuTro")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChuTro")]
        public async Task<IActionResult> Create(PhongTro phongTro)
        {
            if (ModelState.IsValid)
            {
                if (phongTro.ImageUploads != null && phongTro.ImageUploads.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    List<string> uploadedFileNames = new List<string>();

                    foreach (var file in phongTro.ImageUploads)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        uploadedFileNames.Add(uniqueFileName);
                    }
                    phongTro.HinhAnh = string.Join(",", uploadedFileNames);
                }

                var user = await _userManager.GetUserAsync(User);
                phongTro.ChuTroId = user.Id;

                _context.Add(phongTro);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
            return View(phongTro);
        }

        // ĐÃ SỬA: Load thêm danh sách đánh giá từ Database
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phongTro = await _context.PhongTro
                .Include(p => p.ChuTro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phongTro == null) return NotFound();

            // Kéo danh sách đánh giá kèm theo thông tin người đã đánh giá
            ViewBag.DanhGias = await _context.DanhGias
                .Include(d => d.User)
                .Where(d => d.PhongTroId == id)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            return View(phongTro);
        }

        // TÍNH NĂNG MỚI: Nhận và lưu đánh giá vào cơ sở dữ liệu
        [HttpPost]
        public async Task<IActionResult> AddReview(int phongTroId, int sao, string noiDung)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // Đề phòng lỗi

            var danhGia = new DanhGia
            {
                PhongTroId = phongTroId,
                UserId = user.Id,
                Sao = sao,
                NoiDung = noiDung,
                NgayTao = DateTime.Now
            };

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            // Lưu xong thì quay lại đúng trang chi tiết phòng đó
            return RedirectToAction("Details", new { id = phongTroId });
        }
    }
}