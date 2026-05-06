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

        // ========================================================
        // KHAI BÁO MÃ VNPAY
        // ========================================================
        private const string VNP_TMNCODE = "SO3O5OQU";
        private const string VNP_HASHSECRET = "DAATIHR3EIIV5ZRDRIHY8XA7WZ8SDZZI";
        private const string VNP_URL = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        public PhongTroController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // ========================================================
        // 1. ĐĂNG TIN PHÒNG TRỌ (VIP & UPLOAD ẢNH)
        // ========================================================
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

        // ========================================================
        // 2. CHI TIẾT & ĐÁNH GIÁ
        // ========================================================
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

        // ========================================================
        // 3. XỬ LÝ THANH TOÁN VNPAY (THUÊ PHÒNG)
        // ========================================================
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

        // ========================================================
        // 4. QUẢN LÝ & TÌM KIẾM
        // ========================================================
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

            // Tìm kiếm chỉ lấy các phòng chưa cho thuê
            var query = _context.PhongTro.Include(p => p.ChuTro).Where(p => p.DaChoThue == false).AsQueryable();

            if (minPrice.HasValue) query = query.Where(p => p.Gia >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Gia <= maxPrice.Value);

            var listPhong = await query.ToListAsync();
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
                new TruongDaiHoc { Id = "hutech_dbp", TenTruong = "HUTECH (Điện Biên Phủ)", Latitude = 10.8018, Longitude = 106.7115 },
                new TruongDaiHoc { Id = "gtvt", TenTruong = "ĐH Giao thông Vận tải", Latitude = 10.8043, Longitude = 106.7190 },
                new TruongDaiHoc { Id = "spkt", TenTruong = "ĐH Sư Phạm Kỹ Thuật", Latitude = 10.8505, Longitude = 106.7720 }
            };
        }

        // ========================================================
        // 5. LỊCH SỬ THUÊ PHÒNG (DÀNH CHO SINH VIÊN)
        // ========================================================
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

        // ========================================================
        // 6. NẠP TIỀN VÀO VÍ QUA VNPAY (DÀNH CHO CHỦ TRỌ)
        // ========================================================
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

            // Ép UserId vào TxnRef để biết ai nạp tiền
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

            // Lấy ra UserId từ TxnRef
            string userId = txnRef.Split('_')[0];
            // Lấy lại số tiền nạp
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