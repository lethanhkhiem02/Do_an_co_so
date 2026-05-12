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

        // 1. TRANG HỘP THƯ (Bấm từ menu Tin nhắn)z
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

            // Loại bỏ những tin nhắn của Chat Tổng (bắt đầu bằng [GLOBAL_)
            var privateMessages = messages.Where(m => !m.Content.StartsWith("[GLOBAL_")).ToList();

            ViewBag.Receiver = receiver;
            ViewBag.CurrentUserId = currentUser.Id;
            return View(privateMessages);
        }

        // 3. HÀM XỬ LÝ KHI BẤM NÚT GỬI (Chat riêng)
        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var currentUser = await _userManager.GetUserAsync(User);
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

        // 4. TRANG CHAT TỔNG (Lấy dữ liệu để hiển thị ban đầu)
        public async Task<IActionResult> GlobalChat()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Lấy 50 tin nhắn chat tổng gần nhất (bắt đầu bằng [GLOBAL_)
            var rawMessages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.Content.StartsWith("[GLOBAL_"))
                .OrderBy(m => m.Timestamp)
                .Take(50)
                .ToListAsync();

            // 🔥 TÍNH NĂNG MỚI: Ưu tiên lấy HoTen, nếu null thì cắt đuôi @gmail.com
            var processedMessages = rawMessages.Select(m => new {
                SenderName = !string.IsNullOrEmpty(m.Sender?.HoTen) ? m.Sender.HoTen : (m.Sender?.UserName?.Split('@')[0] ?? "Ẩn danh"),
                Type = m.Content.Split(']')[0].Replace("[GLOBAL_", "").ToLower(),
                Content = m.Content.Split(": ", 2)[1],
                Time = m.Timestamp.ToString("HH:mm")
            }).ToList();

            ViewBag.CurrentUserId = currentUser.Id;

            // 🔥 TÍNH NĂNG MỚI: Đổi tên hiển thị cho người đang đăng nhập
            ViewBag.CurrentUserName = !string.IsNullOrEmpty(currentUser.HoTen) ? currentUser.HoTen : (currentUser.UserName?.Split('@')[0] ?? "Ẩn danh");

            return View(processedMessages);
        }

        // 5. API ĐỂ UPLOAD ẢNH (Dùng JavaScript gọi tới)
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
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
    }
}