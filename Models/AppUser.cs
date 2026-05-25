using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Do_an_co_so.Models
{
    public class AppUser : IdentityUser
    {
        public string HoTen { get; set; }
        public string? Avatar { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DiaChi { get; set; }

        [Display(Name = "Số dư ví")]
        public decimal SoDu { get; set; } = 0;

        [Display(Name = "Trạng thái khóa")]
        public bool TrangThaiKhoa { get; set; } = false;

        // =========================================================
        // --- THÊM PHẦN 2: THUỘC TÍNH CHO XÁC THỰC CCCD & OCR ---
        // =========================================================
        [Display(Name = "Ảnh CCCD Mặt Trước")]
        public string? CCCDTruoc { get; set; }

        [Display(Name = "Ảnh CCCD Mặt Sau")]
        public string? CCCDSau { get; set; }

        [Display(Name = "Trạng thái xác thực")]
        public string TrangThaiXacThuc { get; set; } = "Chưa xác thực"; // "Chưa xác thực", "Chờ duyệt", "Đã duyệt"

        [Display(Name = "Số CCCD (AI Quét)")]
        public string? SoCCCDQuetDuoc { get; set; } // AI Tesseract sẽ tự điền vào đây
    }
}