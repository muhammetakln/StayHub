using System;
using System.Collections.Generic;
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
        public List<AmenityDto> Amenities { get; set; } = new();
    }

    public class HotelDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } 
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string? Region { get; set; }
        public string Address { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? HotelType { get; set; }
        public int StarRating { get; set; }
        public int Rating { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public decimal MinPrice { get; set; } = 0m;
        public TimeOnly CheckInTime { get; set; }
        public TimeOnly CheckOutTime { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CoverImageUrl { get; set; }

        public IEnumerable<RoomDto> Rooms { get; set; } = new List<RoomDto>();
        public List<AmenityDto> Amenities { get; set; } = new List<AmenityDto>();
        public IEnumerable<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public List<AddOnServiceDto> AddOnServices { get; set; } = new();
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        public int ActiveReservations { get; set; }
        public decimal MonthlyEarning { get; set; }

    }

    public class CreateHotelDto
    {
        [Required(ErrorMessage = "Otel adı gereklidir")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Şehir gereklidir")]
        public string City { get; set; } = null!;
        public string? Region { get; set; }
        [Required(ErrorMessage = "Ülke gereklidir")]
        public string Country { get; set; } = null!;
        [Required(ErrorMessage = "Adres gereklidir")]
        public string Address { get; set; } = null!;
        [Required(ErrorMessage = "Telefon gereklidir")]
        public string PhoneNumber { get; set; } = null!;
        [Required(ErrorMessage = "Email gereklidir")]
        public string Email { get; set; } = null!;
        public string? Website { get; set; }
        [Display(Name = "Açıklama")]
        [StringLength(1000, ErrorMessage = "Açıklama alanı en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Yıldız sayısı gereklidir")]
        public int StarRating { get; set; }
        public string? HotelType { get; set; }
        public string? Rating { get; set; }
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Otel yönetim paneli için bir şifre belirlemelisiniz.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Şifre 6-20 karakter arası olmalıdır.")]
        [DataType(DataType.Password)]
        public string HotelPassword { get; set; } = null!;
        public TimeOnly CheckInTime { get; set; } = new TimeOnly(14, 0);
        public TimeOnly CheckOutTime { get; set; } = new TimeOnly(11, 0);
        public string? CoverImageUrl { get; set; }
        public List<AddOnServiceDto> AddOnServices { get; set; } = new List<AddOnServiceDto>();

        // SADECE BU KALDI:
        public List<CreateAmenityDto> Amenities { get; set; } = new();
    }

    public class UpdateHotelDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Otel adı gereklidir")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Şehir gereklidir")]
        public string City { get; set; } = null!;
        public string? Region { get; set; }
        [Required(ErrorMessage = "Ülke gereklidir")]
        public string Country { get; set; } = null!;
        [Required(ErrorMessage = "Adres gereklidir")]
        public string Address { get; set; } = null!;
        [Required(ErrorMessage = "Telefon gereklidir")]
        public string PhoneNumber { get; set; } = null!;
        [Required(ErrorMessage = "Email gereklidir")]
        public string Email { get; set; } = null!;
        public string? Website { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(1000, ErrorMessage = "Açıklama alanı en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Yıldız sayısı gereklidir")]
        public int StarRating { get; set; }
        public string? HotelType { get; set; }
        public bool IsActive { get; set; }
        public TimeOnly CheckInTime { get; set; }
        public TimeOnly CheckOutTime { get; set; }
        public string? CoverImageUrl { get; set; }
        public List<UpdateAddOnServiceDto> AddOnServices { get; set; } = new List<UpdateAddOnServiceDto>();

        [Required(ErrorMessage = "Otel yönetim paneli için bir şifre belirlemelisiniz.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Şifre 6-20 karakter arası olmalıdır.")]
        [DataType(DataType.Password)]
        public string HotelPassword { get; set; } = null!;

        // BURASI DEĞİŞTİ (Type UpdateAmenityDto oldu, diğeri silindi):
        public List<UpdateAmenityDto> Amenities { get; set; } = new();
    }

    public class UpdateAddOnServiceDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
    }
}