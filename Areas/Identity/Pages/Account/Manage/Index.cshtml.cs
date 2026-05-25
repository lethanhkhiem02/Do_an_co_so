using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tesseract; // THƯ VIỆN AI NHẬN DIỆN CHỮ OCR

namespace Do_an_co_so.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _env;

        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        public string? Username { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Họ và tên")]
            public string? HoTen { get; set; }

            [Display(Name = "Ngày sinh")]
            [DataType(DataType.Date)]
            public DateTime? NgaySinh { get; set; }

            [Display(Name = "Địa chỉ")]
            public string? DiaChi { get; set; }

            [Display(Name = "Ảnh đại diện")]
            public IFormFile? AvatarFile { get; set; }
            public string? AvatarUrl { get; set; }

            // 🔥 THÊM BIẾN CHO PHẦN UPLOAD CCCD
            [Display(Name = "CCCD Mặt Trước")]
            public IFormFile? CCCDTruocFile { get; set; }

            [Display(Name = "CCCD Mặt Sau")]
            public IFormFile? CCCDSauFile { get; set; }

            public string? CCCDTruocUrl { get; set; }
            public string? CCCDSauUrl { get; set; }
            public string? TrangThaiXacThuc { get; set; }
            public string? SoCCCDQuetDuoc { get; set; }
        }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                HoTen = user.HoTen,
                NgaySinh = user.NgaySinh,
                DiaChi = user.DiaChi,
                AvatarUrl = user.Avatar,
                // Load dữ liệu CCCD cũ lên giao diện
                CCCDTruocUrl = user.CCCDTruoc,
                CCCDSauUrl = user.CCCDSau,
                TrangThaiXacThuc = user.TrangThaiXacThuc,
                SoCCCDQuetDuoc = user.SoCCCDQuetDuoc
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // 🔥 BẢN VÁ LỖI NULL HOTEN: Chỉ cập nhật Profile nếu Form gửi lên có chứa Họ Tên
            if (!string.IsNullOrEmpty(Input.HoTen))
            {
                user.HoTen = Input.HoTen;
                user.NgaySinh = Input.NgaySinh;
                user.DiaChi = Input.DiaChi;

                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (Input.PhoneNumber != phoneNumber)
                {
                    await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                }
            }

            // Xử lý lưu ảnh Avatar
            if (Input.AvatarFile != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.AvatarFile.FileName;
                using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                {
                    await Input.AvatarFile.CopyToAsync(fileStream);
                }
                user.Avatar = uniqueFileName;
            }

            // 🔥 XỬ LÝ UPLOAD CCCD VÀ OCR (TRÍ TUỆ NHÂN TẠO)
            bool isUploadedCCCD = false;
            string cccdPath = "";

            if (Input.CCCDTruocFile != null)
            {
                var cccdFolder = Path.Combine(_env.WebRootPath, "images", "cccd");
                if (!Directory.Exists(cccdFolder)) Directory.CreateDirectory(cccdFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_Truoc_" + Input.CCCDTruocFile.FileName;
                cccdPath = Path.Combine(cccdFolder, uniqueFileName);

                using (var fileStream = new FileStream(cccdPath, FileMode.Create))
                {
                    await Input.CCCDTruocFile.CopyToAsync(fileStream);
                }
                user.CCCDTruoc = uniqueFileName;
                isUploadedCCCD = true;
            }

            if (Input.CCCDSauFile != null)
            {
                var cccdFolder = Path.Combine(_env.WebRootPath, "images", "cccd");
                if (!Directory.Exists(cccdFolder)) Directory.CreateDirectory(cccdFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_Sau_" + Input.CCCDSauFile.FileName;
                using (var fileStream = new FileStream(Path.Combine(cccdFolder, uniqueFileName), FileMode.Create))
                {
                    await Input.CCCDSauFile.CopyToAsync(fileStream);
                }
                user.CCCDSau = uniqueFileName;
                isUploadedCCCD = true;
            }

            // Nếu user có upload CCCD, gọi AI quét và đổi trạng thái
            if (isUploadedCCCD)
            {
                user.TrangThaiXacThuc = "Chờ duyệt"; // Đổi trạng thái

                // GỌI ENGINE TESSERACT OCR ĐỂ ĐỌC CHỮ TRÊN ẢNH MẶT TRƯỚC
                if (!string.IsNullOrEmpty(cccdPath))
                {
                    try
                    {
                        string tessDataPath = Path.Combine(_env.WebRootPath, "tessdata");
                        using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
                        {
                            using (var img = Pix.LoadFromFile(cccdPath))
                            {
                                using (var page = engine.Process(img))
                                {
                                    var text = page.GetText();
                                    // Tìm chuỗi 12 số
                                    var match = Regex.Match(text, @"\b\d{12}\b");
                                    if (match.Success)
                                    {
                                        user.SoCCCDQuetDuoc = match.Value;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi OCR: " + ex.Message);
                    }
                }
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Thao tác thành công!";
            return RedirectToPage();
        }
    }
}