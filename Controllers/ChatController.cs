using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Do_an_co_so.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào phần tin nhắn
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. TRANG HỘP THƯ (Bấm từ menu Tin nhắn)
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // BƯỚC A: Lấy danh sách ID của những người đã nhắn tin (gom nhóm ID để chắc chắn không bị trùng)
            var interactedUserIds = await _context.Messages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .Select(m => m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            // BƯỚC B: Dùng danh sách ID ở trên để tìm ra thông tin chi tiết của họ
            var interactedUsers = await _context.Users
                .Where(u => interactedUserIds.Contains(u.Id))
                .ToListAsync();

            return View(interactedUsers);
        }

        // 2. TRANG KHUNG CHAT CHI TIẾT (Bấm từ nút Nhắn tin ở phòng trọ)
        public async Task<IActionResult> Conversation(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId)) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var receiver = await _userManager.FindByIdAsync(receiverId);

            if (receiver == null) return NotFound();

            // Lấy toàn bộ lịch sử tin nhắn giữa 2 người, sắp xếp theo thời gian
            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            // Gửi dữ liệu phụ sang cho file HTML dùng
            ViewBag.Receiver = receiver;
            ViewBag.CurrentUserId = currentUser.Id;

            return View(messages);
        }

        // 3. HÀM XỬ LÝ KHI BẤM NÚT GỬI
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

            // Gửi xong thì load lại đúng trang chat đó
            return RedirectToAction("Conversation", new { receiverId = receiverId });
        }
    }
}