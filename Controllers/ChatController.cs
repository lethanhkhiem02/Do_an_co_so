using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Do_an_co_so.Controllers
{
    [Authorize] // Bắt buộc đăng nhập
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ChatController(ApplicationDbContext context, UserManager<AppUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // 1. TRANG HỘP THƯ (Bấm từ menu Tin nhắn)
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var interactedUserIds = await _context.Messages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .Select(m => m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();
            var interactedUsers = await _context.Users
                .Where(u => interactedUserIds.Contains(u.Id))
                .ToListAsync();
            return View(interactedUsers);
        }

        // 2. TRANG KHUNG CHAT CHI TIẾT (Chat riêng)
        public async Task<IActionResult> Conversation(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId)) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null) return NotFound();
            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            var privateMessages = messages.Where(m => !m.Content.StartsWith("[GLOBAL_")).ToList();

            ViewBag.Receiver = receiver;
            ViewBag.CurrentUserId = currentUser.Id;
            return View(privateMessages);
        }

        // 3. HÀM XỬ LÝ KHI BẤM NÚT GỬI (Chat riêng)
        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // CHỐT KIỂM TRA: Bị khóa thì chặn cứng, hiện lỗi
            if (currentUser != null && currentUser.TrangThaiKhoa)
            {
                return Content("❌ TÀI KHOẢN CỦA BẠN ĐÃ BỊ KHÓA, KHÔNG THỂ GỬI TIN NHẮN. Vui lòng liên hệ Admin.");
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                var message = new Message
                {
                    SenderId = currentUser.Id,
                    ReceiverId = receiverId,
                    Content = content,
                    Timestamp = DateTime.Now
                };
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Conversation", new { receiverId = receiverId });
        }

        // ============================== PHẦN NÂNG CẤP CHAT TỔNG ==============================

        // 4. TRANG CHAT TỔNG
        public async Task<IActionResult> GlobalChat()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var rawMessages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.Content.StartsWith("[GLOBAL_"))
                .OrderBy(m => m.Timestamp)
                .Take(50)
                .ToListAsync();

            var processedMessages = rawMessages.Select(m => new {
                SenderName = !string.IsNullOrEmpty(m.Sender?.HoTen) ? m.Sender.HoTen : (m.Sender?.UserName?.Split('@')[0] ?? "Ẩn danh"),
                Type = m.Content.Split(']')[0].Replace("[GLOBAL_", "").ToLower(),
                Content = m.Content.Split(": ", 2)[1],
                Time = m.Timestamp.ToString("HH:mm")
            }).ToList();

            ViewBag.CurrentUserId = currentUser.Id;
            ViewBag.CurrentUserName = !string.IsNullOrEmpty(currentUser.HoTen) ? currentUser.HoTen : (currentUser.UserName?.Split('@')[0] ?? "Ẩn danh");

            return View(processedMessages);
        }

        // 5. API ĐỂ UPLOAD ẢNH
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // CHỐT KIỂM TRA: Bị khóa thì không cho gửi ảnh
            if (currentUser != null && currentUser.TrangThaiKhoa)
            {
                return Json(new { success = false, message = "❌ Tài khoản của bạn đã bị khóa, không thể gửi ảnh." });
            }

            if (file == null || file.Length == 0) return Json(new { success = false, message = "Không có file nào được chọn." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension)) return Json(new { success = false, message = "Chỉ cho phép upload file ảnh (.jpg, .png, .gif)." });

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", "chat-images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/chat-images/{fileName}";
            return Json(new { success = true, imageUrl = imageUrl });
        }

        // 6. API LẤY LỊCH SỬ TIN NHẮN CHO MINI CHATBOX (Tại trang chi tiết phòng)
        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string receiverId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.Timestamp)
                .Select(m => new {
                    senderId = m.SenderId,
                    content = m.Content,
                    time = m.Timestamp.ToString("HH:mm")
                })
                .ToListAsync();

            // Lọc bỏ tin nhắn tổng
            var privateMessages = messages.Where(m => !m.content.StartsWith("[GLOBAL_")).ToList();

            return Json(privateMessages);
        }

        // ============================== PHẦN NÂNG CẤP CHỐNG TOXIC ==============================

        // 7. API TIẾP NHẬN BÁO CÁO NGƯỜI DÙNG TỪ KHUNG CHAT
        [HttpPost]
        public async Task<IActionResult> SubmitUserReport(string nguoiBiBaoCaoId, string lyDo, string chiTiet)
        {
            var user = await _userManager.GetUserAsync(User);

            // Nếu chưa đăng nhập hoặc bị khóa tài khoản thì chặn
            if (user == null || user.TrangThaiKhoa)
            {
                TempData["Error"] = "❌ Bạn không có quyền thực hiện thao tác này.";
                return RedirectToAction("Conversation", new { receiverId = nguoiBiBaoCaoId });
            }

            // Tạo một lá đơn báo cáo hướng vào "Tài Khoản" thay vì "Phòng Trọ"
            var report = new BaoCao
            {
                NguoiBiBaoCaoId = nguoiBiBaoCaoId,  // Lưu ID người bị tố cáo
                PhongTroId = null,                  // Để trống ID phòng trọ
                NguoiBaoCaoId = user.Id,
                LyDo = lyDo,
                ChiTiet = chiTiet,
                NgayBaoCao = DateTime.Now,
                DaXuLy = false
            };

            _context.BaoCaos.Add(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "🚩 Cảm ơn bạn! Báo cáo người dùng vi phạm đã được gửi tới Admin xử lý.";

            return RedirectToAction("Conversation", new { receiverId = nguoiBiBaoCaoId });
        }
    }
}