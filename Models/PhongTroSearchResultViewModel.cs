namespace Do_an_co_so.Models
{
    // DTO chứa thông tin Trường học (Đã thêm thuộc tính LoaiTruong)
    public class TruongDaiHoc
    {
        public string Id { get; set; }
        public string TenTruong { get; set; }
        public string Quan { get; set; }
        public string LoaiTruong { get; set; } // Phân loại: "Đại học" hoặc "Cao đẳng"
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    // ViewModel kết hợp Phòng Trọ và Khoảng cách thực tế để ném ra View
    public class PhongTroSearchResultViewModel
    {
        public PhongTro PhongTro { get; set; }
        public double KhoangCach { get; set; }
    }
}