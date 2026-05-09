using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class ReservationDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Otel ID")]
        public int HotelId { get; set; }

        [Required]
        [Display(Name = "Otel Adı")]
        public string HotelName { get; set; } = null!;

        [Display(Name = "Oda ID")]
        public int RoomId { get; set; }

        [Required]
        [Display(Name = "Oda Numarası")]
        public string RoomNumber { get; set; } = null!;

        [Display(Name = "Oda Türü")]
        public string? RoomName { get; set; }

        [Display(Name = "Check-in")]
        [DataType(DataType.DateTime)]
        public DateTime CheckInDate { get; set; }

        [Display(Name = "Check-out")]
        [DataType(DataType.DateTime)]
        public DateTime CheckOutDate { get; set; }

        [Display(Name = "Durum")]
        public string Status { get; set; } = null!;

        [Display(Name = "Toplam Fiyat")]
        public decimal TotalPrice { get; set; }

        public RoomDto? Room { get; set; }

        [Display(Name = "Ekstra Hizmetler")]
        public List<AddOnServiceDto> SelectedServices { get; set; } = new();
    }

    public class ReservationDetailDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Otel ID")]
        public int HotelId { get; set; }

        [Required]
        [Display(Name = "Otel Adı")]
        public string HotelName { get; set; } = null!;

        [Required]
        [Display(Name = "Otel Adresi")]
        public string HotelAddress { get; set; } = null!;

        [Required]
        [Display(Name = "Otel Telefonu")]
        public string HotelPhone { get; set; } = null!;

        [Display(Name = "Oda ID")]
        public int RoomId { get; set; }

        [Required]
        [Display(Name = "Oda Numarası")]
        public string RoomNumber { get; set; } = null!;

        [Required]
        [Display(Name = "Oda Tipi")]
        public string RoomType { get; set; } = null!;

        [Display(Name = "Check-in")]
        [DataType(DataType.DateTime)]
        public DateTime CheckInDate { get; set; }

        [Display(Name = "Check-out")]
        [DataType(DataType.DateTime)]
        public DateTime CheckOutDate { get; set; }

        [Display(Name = "Gece Sayısı")]
        public int NightCount { get; set; }

        [Display(Name = "Durum")]
        public string Status { get; set; } = null!;

        [Display(Name = "Özel İstekler")]
        public string? SpecialRequests { get; set; }

        [Display(Name = "Gecesi Fiyat")]
        public decimal PricePerNight { get; set; }

        [Display(Name = "Ara Toplam")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Vergi")]
        public decimal Tax { get; set; }

        [Display(Name = "Ekstra Hizmetler")]
        public IEnumerable<AddOnServiceDto> AddOnServices { get; set; } = [];

        [Display(Name = "Ekstra Hizmet Toplamı")]
        public decimal AddOnTotal { get; set; }

        [Display(Name = "Genel Toplam")]
        public decimal GrandTotal { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }

    public class ReservationSummaryDto
    {
        [Display(Name = "Oda ID")]
        public int RoomId { get; set; }

        [Required]
        [Display(Name = "Oda Numarası")]
        public string RoomNumber { get; set; } = null!;

        [Required]
        [Display(Name = "Oda Tipi")]
        public string RoomType { get; set; } = null!;

        [Display(Name = "Check-in")]
        [DataType(DataType.DateTime)]
        public DateTime CheckInDate { get; set; }

        [Display(Name = "Check-out")]
        [DataType(DataType.DateTime)]
        public DateTime CheckOutDate { get; set; }

        [Display(Name = "Gece Sayısı")]
        public int NightCount { get; set; }

        [Display(Name = "Gecesi Fiyat")]
        public decimal PricePerNight { get; set; }

        [Display(Name = "Ara Toplam")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Vergi")]
        public decimal Tax { get; set; }

        [Display(Name = "Seçilen Ekstra Hizmetler")]
        public IEnumerable<AddOnServiceDto> SelectedAddOns { get; set; } = [];

        [Display(Name = "Ekstra Hizmet Toplamı")]
        public decimal AddOnTotal { get; set; }

        [Display(Name = "Genel Toplam")]
        public decimal GrandTotal { get; set; }
    }



    public class CreateReservationDto
    {
        [Required(ErrorMessage = "Misafir ID gereklidir")]
        [Display(Name = "Misafir ID")]
        public int GuestId { get; set; }

        [Required(ErrorMessage = "Oda ID gereklidir")]
        [Display(Name = "Oda ID")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Check-in tarihi gereklidir")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Check-in")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out tarihi gereklidir")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Check-out")]
        public DateTime CheckOutDate { get; set; }

        // ✅ EKLEDİĞİMİZ ALAN: Bu alan servisteki hatayı çözecek
        [Required(ErrorMessage = "Misafir sayısı gereklidir")]
        [Range(1, 10, ErrorMessage = "Misafir sayısı 1-10 arasında olmalıdır")]
        [Display(Name = "Misafir Sayısı")]
        public int NumberOf { get; set; }

        [Display(Name = "Ekstra Hizmet ID'leri")]
        public List<int> SelectedAddOnServiceIds { get; set; } = [];

        [StringLength(500, ErrorMessage = "Özel istekler 500 karakteri geçemez")]
        [Display(Name = "Özel İstekler")]
        public string? SpecialRequests { get; set; }

        [Required(ErrorMessage = "Toplam fiyat gereklidir")]
        [Range(0.01, 1000000, ErrorMessage = "Geçerli bir fiyat giriniz")]
        [Display(Name = "Toplam Fiyat")]
        public decimal TotalPrice { get; set; }
        public List<int> SelectedServiceIds { get; set; } = new();
    }
}

public class UpdateReservationDto
{
    [Required(ErrorMessage = "Check-in tarihi gereklidir")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Check-in")]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = "Check-out tarihi gereklidir")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Check-out")]
    public DateTime CheckOutDate { get; set; }

    [StringLength(500, ErrorMessage = "Özel istekler 500 karakteri geçemez")]
    [Display(Name = "Özel İstekler")]
    public string? SpecialRequests { get; set; }

    [Display(Name = "Ekstra Hizmet ID'leri")]
    public List<int> SelectedAddOnServiceIds { get; set; } = [];
}

public class CancelReservationDto
{
    [StringLength(500, ErrorMessage = "İptal sebebi 500 karakteri geçemez")]
    [Display(Name = "İptal Sebebi")]
    public string? CancellationReason { get; set; }
}