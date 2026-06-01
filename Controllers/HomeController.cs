using Do_an_co_so.Data;
using Do_an_co_so.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Do_an_co_so.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

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

        public IActionResult Privacy() { return View(); }
        public IActionResult AboutUs() { return View(); }
        public IActionResult Terms() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ====================================================================
        // 🔥 BỘ NÃO TRỢ LÝ TỰ ĐỘNG - BÁM SÁT 100% DATABASE CỦA ĐỒ ÁN 🔥
        // ====================================================================
        [HttpGet]
        public IActionResult ChatWithAI(string message)
        {
            if (string.IsNullOrEmpty(message)) return Json(new { reply = "Bạn cần mình trợ giúp gì nào?" });

            string msg = message.ToLower().Trim();
            string reply = "";
            Random rand = new Random();

            // 1. KỊCH BẢN CHÀO HỎI
            if (msg.Contains("chào") || msg.Contains("hi") || msg.Contains("hello") || msg.Contains("xin chào"))
            {
                string[] answers = {
                    "👋 Chào bạn! Mình là Trợ lý tự động của RoomFinder. Mình có thể tư vấn cho bạn thông tin về diện tích, tiện ích phòng, quy trình đặt cọc giữ chỗ, hoặc cách dùng bản đồ tìm trọ nha!",
                    "✨ Rất vui được gặp bạn! Bạn cần mình hướng dẫn cách lọc phòng có ban công, nhà vệ sinh riêng hay giải đáp thắc mắc về tiền cọc nè?",
                    "🤖 Xin chào! Mình là chatbot hỗ trợ tự động. Bạn có câu hỏi nào về giá thuê, thời gian giữ phòng hay chính sách tin VIP không?"
                };
                reply = answers[rand.Next(answers.Length)];
            }

            // 2. KỊCH BẢN BẤT NGỜ / TỪ LÓNG
            else if (msg == "hả" || msg.Contains("vailon") || msg == "m" || msg == "gì" || msg == "sao" || msg.Contains("vaiinho"))
            {
                string[] answers = {
                    "🤖 Kìa sếp, bình tĩnh nè! Mình chỉ là Trợ lý tự động thôi. Cứ gõ từ khóa như: 'giá phòng', 'tiện ích', 'đặt cọc' để mình báo thông tin chính xác cho nha!",
                    "💡 Bối rối hả? Hỏi rõ ràng hơn chút về tính năng web hoặc các thông tin phòng (diện tích, vệ sinh riêng...) để mình giải đáp chu đáo nhé!"
                };
                reply = answers[rand.Next(answers.Length)];
            }

            // 3. KỊCH BẢN TÌM TRỌ / BẢN ĐỒ / VỊ TRÍ
            else if (msg.Contains("tìm") || msg.Contains("kiếm trọ") || msg.Contains("bản đồ") || msg.Contains("quanh trường") || msg.Contains("vị trí"))
            {
                reply = "🗺️ Hệ thống tích hợp bản đồ Leaflet thông minh! Qua mục 'Tìm trọ', chọn tên Trường của bạn và bán kính tìm kiếm (km). Bản đồ sẽ tự đo khoảng cách đường chim bay dựa trên tọa độ (Latitude/Longitude) của phòng đến tận cổng trường luôn!";
            }

            // 4. KỊCH BẢN ĐẶT CỌC / GIỮ CHỖ (Dựa trên NguoiDatCocId, TienCoc, HanDatCoc, SoNgayGiuPhong)
            else if (msg.Contains("cọc") || msg.Contains("đặt cọc") || msg.Contains("giữ chỗ") || msg.Contains("giữ phòng") || msg.Contains("vnpay"))
            {
                reply = "💰 Khách có thể thanh toán Tiền Cọc trực tiếp qua VNPay để giữ phòng. Thời gian giữ phòng (số ngày) sẽ do Chủ trọ tự cài đặt. Khi quá Hạn Đặt Cọc mà chưa ký hợp đồng, hệ thống sẽ tự động nhả phòng ra cho người khác thuê để đảm bảo công bằng.";
            }

            // 5. KỊCH BẢN GIÁ CẢ & DIỆN TÍCH & TIỆN ÍCH (Dựa trên Gia, ChieuDai, ChieuRong, CoNhaVeSinh, CoBanCong)
            else if (msg.Contains("giá") || msg.Contains("bao nhiêu") || msg.Contains("tiền") || msg.Contains("rẻ") || msg.Contains("diện tích") || msg.Contains("rộng") || msg.Contains("tiện ích") || msg.Contains("ban công") || msg.Contains("vệ sinh"))
            {
                string[] answers = {
                    "💵 Mọi bài đăng trên RoomFinder đều niêm yết rõ Giá thuê (VNĐ/tháng). Trong chi tiết phòng, bạn có thể xem được kích thước cụ thể (Chiều dài x Chiều rộng) và biết trước phòng đó có Ban công hay Nhà vệ sinh riêng không nha!",
                    "📏 Hệ thống yêu cầu chủ trọ khai báo rõ ràng: Giá tiền, Chiều dài, Chiều rộng của phòng. Bạn cũng có thể dễ dàng thấy phòng có trang bị Nhà vệ sinh riêng hay Ban công không ngay trên giao diện chi tiết."
                };
                reply = answers[rand.Next(answers.Length)];
            }

            // 6. KỊCH BẢN PHÒNG CHỐNG LỪA ĐẢO / HẾT HẠN (Dựa trên DaChoThue, NgayHetHan)
            else if (msg.Contains("lừa đảo") || msg.Contains("uy tín") || msg.Contains("an toàn") || msg.Contains("báo cáo") || msg.Contains("hết hạn") || msg.Contains("phòng ảo"))
            {
                reply = "🚩 Để chống tin rác, các bài đăng đều có 'Ngày hết hạn hiển thị'. Những phòng 'Đã cho thuê' sẽ tự động bị ẩn khỏi danh sách. Nếu phát hiện chủ trọ gian lận thông tin diện tích hay lừa tiền cọc, bạn nhớ bấm Báo cáo cho Admin xử lý nhé!";
            }

            // 7. KỊCH BẢN ĐĂNG TIN VIP (Dựa trên IsVip)
            else if (msg.Contains("vip") || msg.Contains("đăng tin vip") || msg.Contains("phí tin"))
            {
                reply = "⭐ Bài đăng có tích chọn 'Tin VIP' sẽ được ghim nổi bật và ưu tiên hiển thị trên cùng ở cả danh sách lẫn bản đồ. Đây là tính năng rất tiện để chủ trọ tìm khách thuê nhanh chóng hơn!";
            }

            // 8. KỊCH BẢN THÔNG TIN TÁC GIẢ
            else if (msg.Contains("tác giả") || msg.Contains("ai làm") || msg.Contains("phát triển") || msg.Contains("roomfinder"))
            {
                reply = "🏫 Hệ thống RoomFinder được nghiên cứu và phát triển bằng ASP.NET Core MVC nhằm mang lại giải pháp tìm chỗ ở an toàn cho các bạn sinh viên!";
            }

            // 9. TỪ CHỐI KHÉO CÂU HỎI NGOÀI LỀ
            else if (msg.Contains("toán") || msg.Contains("lập trình") || msg.Contains("code") || msg.Contains("thơ") || msg.Contains("lịch sử") || msg.Contains("văn"))
            {
                reply = "🤖 Mình là Trợ lý tự động của hệ thống RoomFinder, nên mình chỉ được lập trình để trả lời các vấn đề xoay quanh hệ thống phòng trọ thôi sếp ơi!";
            }

            // 10. KỊCH BẢN DỰ PHÒNG (FALLBACK)
            else
            {
                string[] fallbacks = {
                    "💡 Mình chưa hiểu ý bạn lắm! Bạn có thể hỏi mình rõ hơn về: giá thuê, diện tích (chiều dài, chiều rộng), tiện ích (ban công, vệ sinh), hoặc thời hạn giữ tiền cọc được không?",
                    "📬 Câu hỏi này ngoài vùng dữ liệu của mình mất rồi! Bạn có thể nhắn tin trực tiếp cho Chủ trọ ở trang chi tiết phòng để trao đổi thêm thông tin nhé.",
                    "🤖 Nếu bạn đang tìm phòng, hãy thử lọc Tỉnh thành, Quận huyện và tên Trường trên thanh tìm kiếm nhé. Hệ thống sẽ tính tọa độ và báo khoảng cách cụ thể cho bạn."
                };
                reply = fallbacks[rand.Next(fallbacks.Length)];
            }

            return Json(new { reply = reply });
        }
    }
}