using Microsoft.AspNetCore.SignalR;
using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Do_an_co_so.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // 1. HÀM DÀNH CHO CHAT TỔNG (GLOBAL CHAT)
        // =========================================================================
        // messageType: "text" hoặc "image"
        // messageContent: nội dung chữ hoặc đường link ảnh
        public async Task SendGlobalMessage(string userId, string userName, string messageType, string messageContent)
        {
            // 1. Lưu vào Database
            var msg = new Message
            {
                SenderId = userId,
                ReceiverId = userId, // Dùng chính userId làm người nhận để né lỗi khóa ngoại
                // Gắn thêm nhãn [GLOBAL_TYPE]: vào nội dung để dễ lọc khi lấy ra
                Content = $"[GLOBAL_{messageType.ToUpper()}]: {messageContent}",
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // 2. Gửi cho tất cả mọi người đang online thấy
            // Gửi một object dữ liệu thay vì gửi chuỗi đơn lẻ cho Frontend dễ xử lý
            var data = new
            {
                User = userName,
                Type = messageType,
                Content = messageContent,
                Time = DateTime.Now.ToString("HH:mm")
            };
            await Clients.All.SendAsync("ReceiveGlobalMessage", data);
        }

        // =========================================================================
        // 2. CÁC HÀM DÀNH CHO CHAT RIÊNG (PRIVATE CHAT 1-1)
        // =========================================================================

        // Hàm gửi tin nhắn 1-1
        public async Task SendPrivateMessage(string senderId, string receiverId, string message)
        {
            // 1. Lưu tin nhắn vào Database
            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // 2. Gửi tin nhắn thẳng tới màn hình của người nhận (nếu họ đang online)
            await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", senderId, message, DateTime.Now.ToString("HH:mm"));

            // 3. Phản hồi lại cho chính người gửi để hiển thị bong bóng chat
            await Clients.Caller.SendAsync("ReceivePrivateMessage", senderId, message, DateTime.Now.ToString("HH:mm"));
        }

        // Hàm thông báo trạng thái "Đang gõ..."
        public async Task SendTypingState(string senderId, string receiverId, bool isTyping)
        {
            // Chỉ gửi thông báo cho đúng người đang chat với mình (người nhận)
            await Clients.User(receiverId).SendAsync("ReceiveTypingState", senderId, isTyping);
        }
    }
}