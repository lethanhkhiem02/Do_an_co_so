using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Do_an_co_so.Services;
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
    [Authorize]
    public class PhongTroController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUser> _userManager;

        private const string VNP_TMNCODE = "SO3O5OQU";
        private const string VNP_HASHSECRET = "DAATIHR3EIIV5ZRDRIHY8XA7WZ8SDZZI";
        private const string VNP_URL = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        public PhongTroController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [Authorize(Roles = "ChuTro")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChuTro")]
        public async Task<IActionResult> Create(PhongTro phongTro)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                phongTro.ChuTroId = user.Id;

                if (phongTro.IsVip)
                {
                    if (user.SoDu < 50000)
                    {
                        ModelState.AddModelError(string.Empty, "❌ Ví không đủ 50.000đ để đăng tin VIP.");
                        return View(phongTro);
                    }
                    user.SoDu -= 50000;
                    await _userManager.UpdateAsync(user);
                }

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

                _context.Add(phongTro);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(phongTro);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var phongTro = await _context.PhongTro.Include(p => p.ChuTro).FirstOrDefaultAsync(m => m.Id == id);
            if (phongTro == null) return NotFound();

            ViewBag.DanhGias = await _context.DanhGias.Include(d => d.User)
                .Where(d => d.PhongTroId == id).OrderByDescending(d => d.NgayTao).ToListAsync();
            return View(phongTro);
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(int phongTroId, int sao, string noiDung)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            _context.DanhGias.Add(new DanhGia
            {
                PhongTroId = phongTroId,
                UserId = user.Id,
                Sao = sao,
                NoiDung = noiDung,
                NgayTao = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = phongTroId });
        }

        [HttpPost]
        [Authorize(Roles = "SinhVien")]
        public async Task<IActionResult> ThanhToanThuePhong(int id)
        {
            var phong = await _context.PhongTro.FirstOrDefaultAsync(p => p.Id == id);
            if (phong == null || phong.DaChoThue) return NotFound();

            string vnp_Returnurl = Url.Action("PaymentCallback", "PhongTro", null, Request.Scheme);
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", VNP_TMNCODE);
            vnpay.AddRequestData("vnp_Amount", ((long)Math.Round(phong.Gia * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan thue phong " + phong.Id);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", phong.Id.ToString() + "_" + DateTime.Now.Ticks);

            string paymentUrl = vnpay.CreateRequestUrl(VNP_URL, VNP_HASHSECRET);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var vnpayData = Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();

            foreach (string s in vnpayData.Keys)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    vnpay.AddResponseData(s, vnpayData[s]);
            }

            int phongId = int.Parse(vnpayData["vnp_TxnRef"].ToString().Split('_')[0]);
            string vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
            string vnp_SecureHash = Request.Query["vnp_SecureHash"];

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, VNP_HASHSECRET);

            if (checkSignature && vnp_ResponseCode == "00")
            {
                var phong = await _context.PhongTro.Include(p => p.ChuTro).FirstOrDefaultAsync(p => p.Id == phongId);
                var nguoiThue = await _userManager.GetUserAsync(User);
                var admin = await _userManager.FindByEmailAsync("admin@gmail.com");

                if (phong != null && !phong.DaChoThue)
                {
                    decimal tongTien = phong.Gia;
                    decimal tienHoaHong = tongTien * 0.10m;
                    decimal tienChuTro = tongTien - tienHoaHong;

                    if (admin != null) admin.SoDu += tienHoaHong;
                    if (phong.ChuTro != null) phong.ChuTro.SoDu += tienChuTro;
                    phong.DaChoThue = true;

                    _context.HoaDons.Add(new HoaDon
                    {
                        PhongTroId = phong.Id,
                        NguoiThueId = nguoiThue?.Id,
                        TongTien = tongTien,
                        TienHoaHong = tienHoaHong,
                        TienChuTroNhan = tienChuTro,
                        NgayGiaoDich = DateTime.Now
                    });

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "🎉 Thuê phòng thành công qua VNPay!";
                }
            }
            else TempData["Error"] = "❌ Giao dịch thất bại hoặc bị hủy.";

            return RedirectToAction("Details", new { id = phongId });
        }

        [Authorize(Roles = "ChuTro")]
        public async Task<IActionResult> QuanLyPhong()
        {
            var user = await _userManager.GetUserAsync(User);
            var ds = await _context.PhongTro.Where(p => p.ChuTroId == user.Id).OrderByDescending(p => p.IsVip).ToListAsync();
            return View(ds);
        }

        [HttpPost]
        [Authorize(Roles = "ChuTro")]
        public async Task<IActionResult> Delete(int id)
        {
            var phong = await _context.PhongTro.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (phong == null || phong.ChuTroId != user.Id) return Forbid();
            _context.PhongTro.Remove(phong);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> TimKiem(string truongId, double? radius, decimal? minPrice, decimal? maxPrice, string sortBy)
        {
            var dsTruong = GetDanhSachTruong();
            ViewBag.DanhSachTruong = dsTruong;

            // LƯU LẠI VIEW BAG ĐỂ GIỮ FORM
            ViewBag.TruongId = truongId;
            ViewBag.Radius = radius;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = string.IsNullOrEmpty(sortBy) ? "distance_asc" : sortBy;

            var query = _context.PhongTro
                .Include(p => p.ChuTro)
                .Where(p => p.DaChoThue == false)
                .AsQueryable();

            if (minPrice.HasValue) query = query.Where(p => p.Gia >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Gia <= maxPrice.Value);

            var listPhong = await query.ToListAsync();

            var phongIds = listPhong.Select(p => p.Id).ToList();
            var saoDict = await _context.DanhGias
                .Where(d => phongIds.Contains(d.PhongTroId))
                .GroupBy(d => d.PhongTroId)
                .Select(g => new { PhongTroId = g.Key, AvgSao = g.Average(d => d.Sao) })
                .ToDictionaryAsync(x => x.PhongTroId, x => x.AvgSao);

            ViewBag.SaoDict = saoDict;

            var ketQua = new List<PhongTroSearchResultViewModel>();
            var truong = dsTruong.FirstOrDefault(t => t.Id == truongId);

            foreach (var p in listPhong)
            {
                double kc = (truong != null && p.Latitude.HasValue && p.Longitude.HasValue)
                    ? TinhKhoangCach(truong.Latitude, truong.Longitude, p.Latitude.Value, p.Longitude.Value) : 0;

                if (!radius.HasValue || kc <= radius.Value)
                    ketQua.Add(new PhongTroSearchResultViewModel { PhongTro = p, KhoangCach = kc });
            }

            ketQua = sortBy switch
            {
                "price_asc" => ketQua.OrderByDescending(k => k.PhongTro.IsVip).ThenBy(k => k.PhongTro.Gia).ToList(),
                "price_desc" => ketQua.OrderByDescending(k => k.PhongTro.IsVip).ThenByDescending(k => k.PhongTro.Gia).ToList(),
                _ => ketQua.OrderByDescending(k => k.PhongTro.IsVip).ThenBy(k => k.KhoangCach).ToList()
            };
            return View(ketQua);
        }

        private double TinhKhoangCach(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return 6371 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private List<TruongDaiHoc> GetDanhSachTruong()
        {
            return new List<TruongDaiHoc> {
                // ======= KHU VỰC BÌNH THẠNH =======
                new TruongDaiHoc { Id = "hutech_dbp", TenTruong = "Đại học HUTECH", Latitude = 10.8018, Longitude = 106.7115, LoaiTruong = "Đại học", Quan = "Bình Thạnh" },
                new TruongDaiHoc { Id = "gtvt", TenTruong = "ĐH Giao thông Vận tải", Latitude = 10.8043, Longitude = 106.7190, LoaiTruong = "Đại học", Quan = "Bình Thạnh" },
                new TruongDaiHoc { Id = "vlu_bt", TenTruong = "Đại học Văn Lang", Latitude = 10.8222, Longitude = 106.6874, LoaiTruong = "Đại học", Quan = "Bình Thạnh" },
                new TruongDaiHoc { Id = "ufm_bt", TenTruong = "ĐH Tài chính - Marketing", Latitude = 10.7961, Longitude = 106.6946, LoaiTruong = "Đại học", Quan = "Bình Thạnh" },
                new TruongDaiHoc { Id = "ftu2", TenTruong = "ĐH Ngoại thương (CS2)", Latitude = 10.8048, Longitude = 106.7169, LoaiTruong = "Đại học", Quan = "Bình Thạnh" },
                new TruongDaiHoc { Id = "uef", TenTruong = "ĐH Kinh tế - Tài chính (UEF)", Latitude = 10.7956, Longitude = 106.7001, LoaiTruong = "Đại học", Quan = "Bình Thạnh" },

                // ======= KHU VỰC THỦ ĐỨC (LÀNG ĐH & LÂN CẬN) =======
                new TruongDaiHoc { Id = "spkt", TenTruong = "ĐH Sư Phạm Kỹ Thuật", Latitude = 10.8505, Longitude = 106.7720, LoaiTruong = "Đại học", Quan = "Thủ Đức" },
                new TruongDaiHoc { Id = "nlu", TenTruong = "ĐH Nông Lâm", Latitude = 10.8697, Longitude = 106.7938, LoaiTruong = "Đại học", Quan = "Thủ Đức" },
                new TruongDaiHoc { Id = "uit", TenTruong = "ĐH Công nghệ Thông tin (UIT)", Latitude = 10.8700, Longitude = 106.8031, LoaiTruong = "Đại học", Quan = "Thủ Đức" },
                new TruongDaiHoc { Id = "hcmus_td", TenTruong = "ĐH Khoa học Tự nhiên", Latitude = 10.8761, Longitude = 106.7979, LoaiTruong = "Đại học", Quan = "Thủ Đức" },
                new TruongDaiHoc { Id = "buh", TenTruong = "ĐH Ngân hàng TP.HCM", Latitude = 10.8566, Longitude = 106.7621, LoaiTruong = "Đại học", Quan = "Thủ Đức" },
                new TruongDaiHoc { Id = "uel", TenTruong = "ĐH Kinh tế - Luật (UEL)", Latitude = 10.8719, Longitude = 106.7984, LoaiTruong = "Đại học", Quan = "Thủ Đức" },
                new TruongDaiHoc { Id = "hcmut_td", TenTruong = "ĐH Bách Khoa (Làng ĐH)", Latitude = 10.8804, Longitude = 106.8053, LoaiTruong = "Đại học", Quan = "Thủ Đức" },
                new TruongDaiHoc { Id = "hcmiu", TenTruong = "ĐH Quốc tế (IU)", Latitude = 10.8732, Longitude = 106.8023, LoaiTruong = "Đại học", Quan = "Thủ Đức" },

                // ======= KHU VỰC QUẬN 1 & QUẬN 3 =======
                new TruongDaiHoc { Id = "ueh_q3", TenTruong = "ĐH Kinh tế TP.HCM (UEH)", Latitude = 10.7828, Longitude = 106.6925, LoaiTruong = "Đại học", Quan = "Quận 3" },
                new TruongDaiHoc { Id = "ussh_q1", TenTruong = "ĐH KHXH & Nhân văn", Latitude = 10.7860, Longitude = 106.7011, LoaiTruong = "Đại học", Quan = "Quận 1" },
                new TruongDaiHoc { Id = "caothang", TenTruong = "CĐ Kỹ thuật Cao Thắng", Latitude = 10.7724, Longitude = 106.7016, LoaiTruong = "Cao đẳng", Quan = "Quận 1" },
                new TruongDaiHoc { Id = "sgu", TenTruong = "Đại học Sài Gòn (SGU)", Latitude = 10.7599, Longitude = 106.6822, LoaiTruong = "Đại học", Quan = "Quận 3" },
                new TruongDaiHoc { Id = "uah", TenTruong = "ĐH Kiến trúc TP.HCM", Latitude = 10.7831, Longitude = 106.6946, LoaiTruong = "Đại học", Quan = "Quận 3" },
                new TruongDaiHoc { Id = "ou", TenTruong = "Đại học Mở TP.HCM", Latitude = 10.7766, Longitude = 106.6917, LoaiTruong = "Đại học", Quan = "Quận 3" },

                // ======= KHU VỰC QUẬN 5 & QUẬN 10 =======
                new TruongDaiHoc { Id = "hcmut_q10", TenTruong = "ĐH Bách Khoa TP.HCM", Latitude = 10.7732, Longitude = 106.6597, LoaiTruong = "Đại học", Quan = "Quận 10" },
                new TruongDaiHoc { Id = "huflit", TenTruong = "ĐH Ngoại ngữ - Tin học (HUFLIT)", Latitude = 10.7765, Longitude = 106.6669, LoaiTruong = "Đại học", Quan = "Quận 10" },
                new TruongDaiHoc { Id = "hcmus_q5", TenTruong = "ĐH Khoa học Tự nhiên", Latitude = 10.7630, Longitude = 106.6821, LoaiTruong = "Đại học", Quan = "Quận 5" },
                new TruongDaiHoc { Id = "hcmue", TenTruong = "ĐH Sư Phạm TP.HCM", Latitude = 10.7613, Longitude = 106.6822, LoaiTruong = "Đại học", Quan = "Quận 5" },
                new TruongDaiHoc { Id = "ump", TenTruong = "ĐH Y Dược TP.HCM", Latitude = 10.7562, Longitude = 106.6661, LoaiTruong = "Đại học", Quan = "Quận 5" },
                new TruongDaiHoc { Id = "pnt", TenTruong = "ĐH Y khoa Phạm Ngọc Thạch", Latitude = 10.7745, Longitude = 106.6668, LoaiTruong = "Đại học", Quan = "Quận 10" },

                // ======= KHU VỰC QUẬN 7 =======
                new TruongDaiHoc { Id = "tdt", TenTruong = "ĐH Tôn Đức Thắng", Latitude = 10.7325, Longitude = 106.6983, LoaiTruong = "Đại học", Quan = "Quận 7" },
                new TruongDaiHoc { Id = "rmit", TenTruong = "ĐH RMIT Việt Nam", Latitude = 10.7293, Longitude = 106.6946, LoaiTruong = "Đại học", Quan = "Quận 7" },

                // ======= CÁC QUẬN KHÁC (GÒ VẤP, TÂN BÌNH, QUẬN 12...) =======
                new TruongDaiHoc { Id = "iuh", TenTruong = "ĐH Công nghiệp TP.HCM (IUH)", Latitude = 10.8222, Longitude = 106.6875, LoaiTruong = "Đại học", Quan = "Gò Vấp" },
                new TruongDaiHoc { Id = "fpt_poly", TenTruong = "CĐ FPT Polytechnic", Latitude = 10.8531, Longitude = 106.6263, LoaiTruong = "Cao đẳng", Quan = "Quận 12" },
                new TruongDaiHoc { Id = "viendong", TenTruong = "CĐ Viễn Đông", Latitude = 10.8549, Longitude = 106.6277, LoaiTruong = "Cao đẳng", Quan = "Quận 12" },
                new TruongDaiHoc { Id = "huit", TenTruong = "ĐH Công thương (HUIT)", Latitude = 10.8066, Longitude = 106.6288, LoaiTruong = "Đại học", Quan = "Tân Phú" }
            };
        }

        [Authorize(Roles = "SinhVien")]
        public async Task<IActionResult> PhongDaThue()
        {
            var user = await _userManager.GetUserAsync(User);

            var danhSachThue = await _context.HoaDons
                .Include(h => h.PhongTro)
                .ThenInclude(p => p.ChuTro)
                .Where(h => h.NguoiThueId == user.Id)
                .OrderByDescending(h => h.NgayGiaoDich)
                .ToListAsync();

            return View(danhSachThue);
        }

        [Authorize(Roles = "ChuTro")]
        [HttpGet]
        public IActionResult NapTien()
        {
            return View();
        }

        [Authorize(Roles = "ChuTro")]
        [HttpPost]
        public async Task<IActionResult> XuLyNapTien(decimal soTien)
        {
            if (soTien < 10000)
            {
                TempData["Error"] = "❌ Số tiền nạp tối thiểu là 10.000đ";
                return RedirectToAction("NapTien");
            }

            var user = await _userManager.GetUserAsync(User);

            string vnp_Returnurl = Url.Action("NapTienCallback", "PhongTro", null, Request.Scheme);
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", VNP_TMNCODE);
            vnpay.AddRequestData("vnp_Amount", ((long)Math.Round(soTien * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Nap tien vao vi VIP - " + user.UserName);
            vnpay.AddRequestData("vnp_OrderType", "topup");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);

            vnpay.AddRequestData("vnp_TxnRef", user.Id + "_" + DateTime.Now.Ticks);

            string paymentUrl = vnpay.CreateRequestUrl(VNP_URL, VNP_HASHSECRET);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public async Task<IActionResult> NapTienCallback()
        {
            var vnpayData = Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();

            foreach (string s in vnpayData.Keys)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    vnpay.AddResponseData(s, vnpayData[s]);
            }

            string vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
            string vnp_SecureHash = Request.Query["vnp_SecureHash"];
            string txnRef = vnpayData["vnp_TxnRef"].ToString();

            string userId = txnRef.Split('_')[0];
            decimal soTienNap = decimal.Parse(vnpayData["vnp_Amount"]) / 100;

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, VNP_HASHSECRET);

            if (checkSignature && vnp_ResponseCode == "00")
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.SoDu += soTienNap;
                    await _userManager.UpdateAsync(user);
                    TempData["Success"] = $"🎉 Nạp thành công {soTienNap.ToString("N0")}đ vào ví!";
                }
            }
            else
            {
                TempData["Error"] = "❌ Nạp tiền thất bại hoặc giao dịch bị hủy.";
            }

            return RedirectToAction("QuanLyPhong", "PhongTro");
        }
    }
}