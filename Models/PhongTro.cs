using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Thư viện để dùng biến IFormFile tải ảnh

namespace Do_an_co_so.Models
{
    public class PhongTro
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [Display(Name = "Mô tả phòng")]
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá phòng")]
        [Display(Name = "Giá thuê (VNĐ/Tháng)")]
        public decimal Gia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string DiaChi { get; set; }

        [Display(Name = "Hình ảnh minh họa")]
        public string? HinhAnh { get; set; } // Tên file ảnh sẽ lưu vào Database

        [NotMapped] // Khai báo này báo cho hệ thống biết KHÔNG tạo cột này trong CSDL
        [Display(Name = "Chọn các ảnh tải lên")]
        public List<IFormFile>? ImageUploads { get; set; } // Đã chuyển thành List để nhận nhiều ảnh

        // --- Liên kết để biết phòng này do Chủ Trọ nào đăng ---
        public string? ChuTroId { get; set; }
        [ForeignKey("ChuTroId")]
        public AppUser? ChuTro { get; set; }

        [Display(Name = "Vĩ độ (Latitude)")]
        public double? Latitude { get; set; }

        [Display(Name = "Kinh độ (Longitude)")]
        public double? Longitude { get; set; }

        // --- CÁC THUỘC TÍNH MỚI CHO TÍNH NĂNG VIP ---
        [Display(Name = "Tin VIP")]
        public bool IsVip { get; set; } = false; // Mặc định đăng lên là tin thường

        // --- THUỘC TÍNH MỚI: KIỂM TRA PHÒNG ĐÃ THUÊ CHƯA ---
        [Display(Name = "Trạng thái thuê")]
        public bool DaChoThue { get; set; } = false; // false = Còn trống, true = Đã có người thuê

        // ==============================================================================
        // --- THÊM 4 THUỘC TÍNH MỚI CHO CHI TIẾT PHÒNG TRỌ (BƯỚC 1) ---
        // ==============================================================================

        [Display(Name = "Chiều dài (m)")]
        [Range(0, 100, ErrorMessage = "Chiều dài phải là số dương")]
        public double? ChieuDai { get; set; } // Dùng double? (có dấu ?) để tránh lỗi với các bài đăng cũ

        [Display(Name = "Chiều rộng (m)")]
        [Range(0, 100, ErrorMessage = "Chiều rộng phải là số dương")]
        public double? ChieuRong { get; set; }

        [Display(Name = "Có nhà vệ sinh riêng")]
        public bool CoNhaVeSinh { get; set; } = false; // Mặc định là Không (false)

        [Display(Name = "Có ban công")]
        public bool CoBanCong { get; set; } = false; // Mặc định là Không (false)

        // ==============================================================================
        // --- THÊM CÁC THUỘC TÍNH CHO TÍNH NĂNG ĐẶT CỌC GIỮ CHỖ ---
        // ==============================================================================

        [Display(Name = "Người đặt cọc")]
        public string? NguoiDatCocId { get; set; } // Lưu ID người cọc để khóa phòng với người khác

        [Display(Name = "Tiền cọc")]
        public decimal? TienCoc { get; set; } // Lưu số tiền đã cọc (500k)

        [Display(Name = "Hạn đặt cọc")]
        public DateTime? HanDatCoc { get; set; } // Hạn chót để đóng phần còn lại (7 ngày)
    }
}