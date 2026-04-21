using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_an_co_so.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        // Ai là người gửi?
        public string? SenderId { get; set; }
        [ForeignKey("SenderId")]
        public AppUser? Sender { get; set; }

        // Ai là người nhận?
        public string? ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public AppUser? Receiver { get; set; }

        [Required]
        public string Content { get; set; } // Nội dung tin nhắn

        public DateTime Timestamp { get; set; } = DateTime.Now; // Thời gian gửi
    }
}