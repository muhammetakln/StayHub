using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class HotelDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Otel Adı")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Şehir")]
        public string City { get; set; } = null!;

        [Required]
        [Display(Name = "Ülke")]
        public string Country { get; set; } = null!;
        public string? HotelType { get; set; }

        [Display(Name = "Yıldız Sayısı")]
        public string? Rating { get; set; }

        [Display(Name = "Minimum Fiyat")]
        public decimal MinPrice { get; set; } = 0m;

        [Display(Name = "Ortalama Puan")]
        public double AverageRating { get; set; }

        [Display(Name = "Yorum Sayısı")]
        public int ReviewCount { get; set; }

        [Display(Name = "Kapak Resmi")]
        public string? CoverImageUrl { get; set; }
        [Display(Name = "aktif mi otel?")]
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    public class HotelDetailDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Otel Adı")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = null!;

        [Required]
        [Display(Name = "Şehir")]
        public string City { get; set; } = null!;

        [Required]
        [Display(Name = "Ülke")]
        public string Country { get; set; } = null!;

        [Display(Name = "Bölge")]
        public string? Region { get; set; }

        [Required]
        [Display(Name = "Adres")]
        public string Address { get; set; } = null!;

        [Required]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Otel Tipi")]
        public string? HotelType { get; set; }

        [Display(Name = "Yıldız Sayısı")]
        public int StarRating { get; set; }

        [Display(Name = "Rating")]
        public string? Rating { get; set; }

        [Display(Name = "Ortalama Puan")]
        public double AverageRating { get; set; }

        [Display(Name = "Yorum Sayısı")]
        public int ReviewCount { get; set; }

        [Display(Name = "Başlangıç Fiyatı")]
        public decimal MinPrice { get; set; } = 0m;

        [Display(Name = "Giriş Saati")]
        public string CheckInTime { get; set; } = "14:00";

        [Display(Name = "Çıkış Saati")]
        public string CheckOutTime { get; set; } = "11:00";

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Kapak Resmi")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Odalar")]
        public IEnumerable<RoomDto> Rooms { get; set; } = new List<RoomDto>();

        [Display(Name = "Olanaklar")]
        public IEnumerable<AmenityDto> Amenities { get; set; } = new List<AmenityDto>();

        [Display(Name = "Yorumlar")]
        public IEnumerable<ReviewListDto> Reviews { get; set; } = new List<ReviewListDto>();
    }

    public class HotelSummaryDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Otel Adı")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Şehir")]
        public string City { get; set; } = null!;

        [Display(Name = "Minimum Fiyat")]
        public decimal MinPrice { get; set; }

        [Display(Name = "Puan")]
        public double Rating { get; set; }

        [Display(Name = "Resim")]
        public string? ImageUrl { get; set; }
    }

    public class CreateHotelDto
    {
        [Required(ErrorMessage = "Otel adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Otel adı 3-100 karakter arasında olmalıdır")]
        [Display(Name = "Otel Adı")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Şehir gereklidir")]
        [StringLength(50, ErrorMessage = "Şehir maksimum 50 karakter olmalıdır")]
        [Display(Name = "Şehir")]
        public string City { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Bölge maksimum 50 karakter olmalıdır")]
        [Display(Name = "Bölge")]
        public string Region { get; set; }

        [Required(ErrorMessage = "Ülke gereklidir")]
        [StringLength(50, ErrorMessage = "Ülke maksimum 50 karakter olmalıdır")]
        [Display(Name = "Ülke")]
        public string Country { get; set; } = null!;

        [Required(ErrorMessage = "Adres gereklidir")]
        [StringLength(200, ErrorMessage = "Adres maksimum 200 karakter olmalıdır")]
        [Display(Name = "Adres")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Telefon gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Email gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        [StringLength(200, ErrorMessage = "Website maksimum 200 karakter olmalıdır")]
        [Display(Name = "Website")]
        public string Website { get; set; }

        [Required(ErrorMessage = "Açıklama gereklidir")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Açıklama 10-1000 karakter arasında olmalıdır")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Yıldız sayısı gereklidir")]
        [Range(1, 5, ErrorMessage = "Yıldız sayısı 1-5 arasında olmalıdır")]
        [Display(Name = "Yıldız Sayısı")]
        public int StarRating { get; set; }

        [StringLength(50, ErrorMessage = "Otel tipi maksimum 50 karakter olmalıdır")]
        [Display(Name = "Otel Tipi")]
        public string? HotelType { get; set; }
        public string? Rating { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }
}

public class UpdateHotelDto
{
    [Required(ErrorMessage = "Otel adı gereklidir")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Otel adı 3-100 karakter arasında olmalıdır")]
    [Display(Name = "Otel Adı")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Şehir gereklidir")]
    [StringLength(50, ErrorMessage = "Şehir maksimum 50 karakter olmalıdır")]
    [Display(Name = "Şehir")]
    public string City { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Bölge maksimum 50 karakter olmalıdır")]
    [Display(Name = "Bölge")]
    public string Region { get; set; }

    [Required(ErrorMessage = "Ülke gereklidir")]
    [StringLength(50, ErrorMessage = "Ülke maksimum 50 karakter olmalıdır")]
    [Display(Name = "Ülke")]
    public string Country { get; set; } = null!;

    [Required(ErrorMessage = "Adres gereklidir")]
    [StringLength(200, ErrorMessage = "Adres maksimum 200 karakter olmalıdır")]
    [Display(Name = "Adres")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Telefon gereklidir")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [Display(Name = "Telefon")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Email gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    [StringLength(200, ErrorMessage = "Website maksimum 200 karakter olmalıdır")]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [Required(ErrorMessage = "Açıklama gereklidir")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Açıklama 10-1000 karakter arasında olmalıdır")]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Yıldız sayısı gereklidir")]
    [Range(1, 5, ErrorMessage = "Yıldız sayısı 1-5 arasında olmalıdır")]
    [Display(Name = "Yıldız Sayısı")]
    public int StarRating { get; set; }

    [StringLength(50, ErrorMessage = "Otel tipi maksimum 50 karakter olmalıdır")]
    [Display(Name = "Otel Tipi")]
    public string HotelType { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }
}

