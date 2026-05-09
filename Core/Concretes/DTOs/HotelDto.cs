using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Core.Concretes.DTOs.AddOnServiceDto;

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
        public int StarRating { get; set; }

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

        [Display(Name = "Giriş Saati")]
        public TimeOnly CheckInTime { get; set; } = new TimeOnly(14, 0);

        [Display(Name = "Çıkış Saati")]
        public TimeOnly CheckOutTime { get; set; } = new TimeOnly(11, 0);
        public List<AddOnServiceDto> AddOnServices { get; set; } = new List<AddOnServiceDto>();
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

        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Otel Tipi")]
        public string? HotelType { get; set; }

        [Display(Name = "Yıldız Sayısı")]
        public int StarRating { get; set; }

        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [Display(Name = "Ortalama Puan")]
        public double AverageRating { get; set; }

        [Display(Name = "Yorum Sayısı")]
        public int ReviewCount { get; set; }

        [Display(Name = "Başlangıç Fiyatı")]
        public decimal MinPrice { get; set; } = 0m;

        [Display(Name = "Giriş Saati")]
        public TimeOnly CheckInTime { get; set; }

        [Display(Name = "Çıkış Saati")]
        public TimeOnly CheckOutTime { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Kapak Resmi")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Odalar")]
        public IEnumerable<RoomDto> Rooms { get; set; } = new List<RoomDto>();

        [Display(Name = "Olanaklar")]
        public IEnumerable<AmenityDto> Amenities { get; set; } = new List<AmenityDto>();

        [Display(Name = "Yorumlar")]
        public IEnumerable<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public List<AddOnServiceDto> AddOnServices { get; set; } = new();
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        public int ActiveReservations { get; set; }
        public decimal MonthlyEarning { get; set; }
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
        public string? Region { get; set; }

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
        public string? HotelType { get; set; }
        public string? Rating { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Giriş Saati")]
        public TimeOnly CheckInTime { get; set; } = new TimeOnly(14, 0);

        [Display(Name = "Çıkış Saati")]
        public TimeOnly CheckOutTime { get; set; } = new TimeOnly(11, 0);

        [Display(Name = "Kapak Görseli URL")]
        public string? CoverImageUrl { get; set; }
        public List<AddOnServiceDto> AddOnServices { get; set; } = new List<AddOnServiceDto>();
    }

    public class UpdateHotelDto
    {
        public int Id { get; set; }
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
        public string? Region { get; set; }

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
        public string? HotelType { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Giriş Saati")]
        public TimeOnly CheckInTime { get; set; }

        [Display(Name = "Çıkış Saati")]
        public TimeOnly CheckOutTime { get; set; }

        [Display(Name = "Kapak Görseli URL")]
        public string? CoverImageUrl { get; set; }
        public List<UpdateAddOnServiceDto> AddOnServices { get; set; } = new List<UpdateAddOnServiceDto>();
    }
    public class UpdateAddOnServiceDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
    }
}