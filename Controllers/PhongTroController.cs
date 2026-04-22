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

        [AllowAnonymous] // Cho phép khách vãng lai xem chi tiết phòng
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

            return RedirectToAction("Details", new { id = phongTroId });
        }

        // =====================================================================
        // CHỨC NĂNG TÌM KIẾM THEO BÁN KÍNH VÀ LOẠI TRƯỜNG
        // =====================================================================

        // Danh sách siêu to khổng lồ các trường Đại học/Cao đẳng (In-Memory)
        private List<TruongDaiHoc> GetDanhSachTruong()
        {
            return new List<TruongDaiHoc>
            {
                // ============ HỆ ĐẠI HỌC ============
                new TruongDaiHoc { Id = "hutech_dbp", TenTruong = "HUTECH (Điện Biên Phủ)", LoaiTruong = "Đại học", Quan = "Bình Thạnh", Latitude = 10.8018, Longitude = 106.7115 },
                new TruongDaiHoc { Id = "gtvt", TenTruong = "ĐH Giao thông Vận tải", LoaiTruong = "Đại học", Quan = "Bình Thạnh", Latitude = 10.8043, Longitude = 106.7190 },
                new TruongDaiHoc { Id = "ngoai_thuong", TenTruong = "ĐH Ngoại Thương (CS2)", LoaiTruong = "Đại học", Quan = "Bình Thạnh", Latitude = 10.8048, Longitude = 106.7163 },
                new TruongDaiHoc { Id = "hutech_khue", TenTruong = "HUTECH (Khu E)", LoaiTruong = "Đại học", Quan = "Thủ Đức", Latitude = 10.8496, Longitude = 106.7766 },
                new TruongDaiHoc { Id = "spkt", TenTruong = "ĐH Sư Phạm Kỹ Thuật", LoaiTruong = "Đại học", Quan = "Thủ Đức", Latitude = 10.8505, Longitude = 106.7720 },
                new TruongDaiHoc { Id = "fpt_uni", TenTruong = "Đại học FPT", LoaiTruong = "Đại học", Quan = "Thủ Đức", Latitude = 10.8411, Longitude = 106.8098 },
                new TruongDaiHoc { Id = "bachkhoa", TenTruong = "ĐH Bách Khoa", LoaiTruong = "Đại học", Quan = "Quận 10", Latitude = 10.7733, Longitude = 106.6597 },
                new TruongDaiHoc { Id = "khtn", TenTruong = "ĐH Khoa học Tự nhiên", LoaiTruong = "Đại học", Quan = "Quận 5", Latitude = 10.7631, Longitude = 106.6823 },
                new TruongDaiHoc { Id = "tdt", TenTruong = "ĐH Tôn Đức Thắng", LoaiTruong = "Đại học", Quan = "Quận 7", Latitude = 10.7326, Longitude = 106.6997 },
                new TruongDaiHoc { Id = "rmit", TenTruong = "ĐH RMIT Nam Sài Gòn", LoaiTruong = "Đại học", Quan = "Quận 7", Latitude = 10.7293, Longitude = 106.6942 },

                // ============ HỆ CAO ĐẲNG ============
                new TruongDaiHoc { Id = "fpt_poly", TenTruong = "Cao đẳng FPT Polytechnic", LoaiTruong = "Cao đẳng", Quan = "Phú Nhuận", Latitude = 10.7908, Longitude = 106.6823 },
                new TruongDaiHoc { Id = "cd_kinhte", TenTruong = "Cao đẳng Kinh tế Đối ngoại", LoaiTruong = "Cao đẳng", Quan = "Phú Nhuận", Latitude = 10.7972, Longitude = 106.6806 },
                new TruongDaiHoc { Id = "cd_congthuong", TenTruong = "Cao đẳng Công Thương", LoaiTruong = "Cao đẳng", Quan = "Thủ Đức", Latitude = 10.8268, Longitude = 106.7303 },
                new TruongDaiHoc { Id = "cd_caothang", TenTruong = "Cao đẳng Kỹ thuật Cao Thắng", LoaiTruong = "Cao đẳng", Quan = "Quận 1", Latitude = 10.7725, Longitude = 106.7011 },
                new TruongDaiHoc { Id = "cd_viendong", TenTruong = "Cao đẳng Viễn Đông", LoaiTruong = "Cao đẳng", Quan = "Quận 12", Latitude = 10.8524, Longitude = 106.6275 }
            };
        }

        [AllowAnonymous]
        public async Task<IActionResult> TimKiem(string truongId, double? radius, decimal? minPrice, decimal? maxPrice, string sortBy)
        {
            // 1. Nạp danh sách trường ra View để làm Dropdown
            var dsTruong = GetDanhSachTruong();
            ViewBag.DanhSachTruong = dsTruong;

            // Giữ lại lựa chọn của người dùng trên thanh tìm kiếm
            ViewBag.TruongId = truongId;
            ViewBag.Radius = radius;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;

            var query = _context.PhongTro.Include(p => p.ChuTro).AsQueryable();

            // 2. Lọc theo khoảng giá
            if (minPrice.HasValue) query = query.Where(p => p.Gia >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Gia <= maxPrice.Value);

            var listPhong = await query.ToListAsync();
            var ketQua = new List<PhongTroSearchResultViewModel>();

            var truongDuocChon = dsTruong.FirstOrDefault(t => t.Id == truongId);

            // 3. Tính khoảng cách và lọc theo bán kính
            if (truongDuocChon != null && radius.HasValue)
            {
                foreach (var p in listPhong)
                {
                    if (p.Latitude.HasValue && p.Longitude.HasValue)
                    {
                        double kc = TinhKhoangCach(truongDuocChon.Latitude, truongDuocChon.Longitude, p.Latitude.Value, p.Longitude.Value);

                        // Chỉ lấy phòng nằm trong bán kính đã chọn
                        if (kc <= radius.Value)
                        {
                            ketQua.Add(new PhongTroSearchResultViewModel
                            {
                                PhongTro = p,
                                KhoangCach = kc
                            });
                        }
                    }
                }
            }
            else
            {
                // Nếu người dùng không chọn mốc trường học, hiển thị danh sách bình thường
                foreach (var p in listPhong)
                {
                    ketQua.Add(new PhongTroSearchResultViewModel { PhongTro = p, KhoangCach = 0 });
                }
            }

            // 4. Sắp xếp kết quả (Sorting)
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "distance_asc":
                        ketQua = ketQua.OrderBy(k => k.KhoangCach).ToList();
                        break;
                    case "price_asc":
                        ketQua = ketQua.OrderBy(k => k.PhongTro.Gia).ToList();
                        break;
                    case "price_desc":
                        ketQua = ketQua.OrderByDescending(k => k.PhongTro.Gia).ToList();
                        break;
                }
            }
            else
            {
                // Mặc định: Nếu có mốc trường thì xếp từ gần đến xa, không thì xếp bài mới nhất lên đầu
                if (truongDuocChon != null)
                    ketQua = ketQua.OrderBy(k => k.KhoangCach).ToList();
                else
                    ketQua = ketQua.OrderByDescending(k => k.PhongTro.Id).ToList();
            }

            return View(ketQua);
        }

        // THUẬT TOÁN HAVERSINE: Tính khoảng cách đường chim bay (bằng km) giữa 2 tọa độ GPS
        private double TinhKhoangCach(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371d; // Bán kính Trái Đất theo km
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Trả về khoảng cách (km)
        }
    }
}