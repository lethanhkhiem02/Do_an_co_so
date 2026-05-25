using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json.Nodes; // 🔥 Thư viện xử lý JsonNode để chống lỗi candidates

namespace Do_an_co_so.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        // Hàm khởi tạo đã tiêm đầy đủ Logger, DbContext và Configuration
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            // Ẩn phòng đã thuê VÀ ẩn bài đăng đã hết hạn hiển thị công khai trên hệ thống
            var danhSachPhong = await _context.PhongTro
                .Where(p => p.DaChoThue == false && (p.NgayHetHan == null || p.NgayHetHan >= DateTime.Now))
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

        // ==========================================
        // CÁC HÀM DÀNH CHO FOOTER
        // ==========================================
        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ==========================================
        // 🔥 ACTION GỌI AI GEMINI THẬT 100% 
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ChatWithAI(string message)
        {
            if (string.IsNullOrEmpty(message)) return Json(new { reply = "Bạn cần hỏi gì nào?" });

            var apiKey = _configuration["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return Json(new { reply = "⚠️ Lỗi: Chưa tìm thấy API Key trong file appsettings.json!" });

            // Sử dụng Model đời mới 2.5-flash theo cập nhật hệ thống Google
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            // Câu lệnh nhập vai kỷ luật sắt - Tuyệt đối không chứa từ khóa trường học cũ
            string systemPrompt = "Bạn là nhân viên tư vấn phòng trọ ảo của website RoomFinder do Nguyễn Văn Tính phát triển. Nhiệm vụ của bạn LÀ DUY NHẤT trả lời các vấn đề liên quan đến thuê phòng, giá cả, hợp đồng, lừa đảo, cọc tiền hoặc hướng dẫn dùng web RoomFinder. NẾU người dùng hỏi các chủ đề ngoài lề (toán học, lịch sử, lập trình, làm thơ, v.v.), BẮT BUỘC phải từ chối khéo léo và yêu cầu họ quay lại chủ đề phòng trọ. Luôn xưng 'mình' và gọi người dùng là 'bạn'. Trả lời thân thiện, ngắn gọn dưới 3 câu. Câu hỏi của người dùng là: ";

            using (var client = new HttpClient())
            {
                var payload = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = systemPrompt + message } } }
                    }
                };

                try
                {
                    var response = await client.PostAsJsonAsync(url, payload);
                    if (response.IsSuccessStatusCode)
                    {
                        // Bóc tách JSON bằng JsonNode an toàn, chống crash luồng dữ liệu
                        var jsonNode = await response.Content.ReadFromJsonAsync<JsonNode>();
                        string aiReply = jsonNode["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                        return Json(new { reply = aiReply?.Trim() });
                    }
                    else
                    {
                        // Xử lý thẩm mỹ lỗi quá tải tần suất gửi tin nhắn (503 / 429) của Google
                        if ((int)response.StatusCode == 503 || (int)response.StatusCode == 429)
                        {
                            return Json(new { reply = "⚠️ Máy chủ AI hiện đang có quá nhiều người truy cập cùng lúc. Bạn vui lòng đợi khoảng 10 giây rồi gửi lại tin nhắn nhé!" });
                        }

                        return Json(new { reply = $"⚠️ Trợ lý AI đang tạm nghỉ một chút. (Mã lỗi: {response.StatusCode})" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { reply = "⚠️ Hệ thống mất kết nối mạng, vui lòng thử lại sau ít phút!" });
                }
            }
        }
    }
}