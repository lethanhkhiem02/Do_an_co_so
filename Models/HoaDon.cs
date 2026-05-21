using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Do_an_co_so.Models
{
    public class HoaDon
    {
        [Key]
        public int Id { get; set; }

        // Hóa đơn này dành cho phòng trọ nào?
        public int PhongTroId { get; set; }
        [ForeignKey("PhongTroId")]
        public PhongTro PhongTro { get; set; }

        // Ai là người thuê (thanh toán) phòng này?
        public string NguoiThueId { get; set; }
        [ForeignKey("NguoiThueId")]
        public AppUser NguoiThue { get; set; }

        // Thông tin tiền bạc
        public decimal TongTien { get; set; }       // Ví dụ: Giá phòng 2.000.000đ
        public decimal TienHoaHong { get; set; }    // Ví dụ: Admin thu 10% = 200.000đ
        public decimal TienChuTroNhan { get; set; } // Ví dụ: Chủ trọ nhận 1.800.000đ

        public DateTime NgayGiaoDich { get; set; } = DateTime.Now;

        // --- THÊM THUỘC TÍNH PHÂN BIỆT LOẠI HÓA ĐƠN ---
        [Display(Name = "Loại hóa đơn")]
        public string LoaiHoaDon { get; set; } = "ThuePhong"; // Mặc định là "ThuePhong", nếu cọc thì sửa thành "DatCoc"
    }
}