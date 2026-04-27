using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class AdminDashboardDto
    {
        [Display(Name = "İstatistikler")]
        [Required(ErrorMessage = "İstatistikler gereklidir")]
        public DashboardStatsDto Stats { get; set; } = new DashboardStatsDto();

        [Display(Name = "Oteller")]
        [Required(ErrorMessage = "Otel listesi gereklidir")]
        public List<HotelDto> Hotels { get; set; } = new List<HotelDto>();

        [Display(Name = "Son Rezervasyonlar")]
        [Required(ErrorMessage = "Rezervasyon listesi gereklidir")]
        public List<ReservationDetailDto> RecentReservations { get; set; } = new List<ReservationDetailDto>();

        [Display(Name = "Müşteriler")]
        [Required(ErrorMessage = "Müşteri listesi gereklidir")]
        public List<GuestSummaryDto> Guests { get; set; } = new List<GuestSummaryDto>();

        [Display(Name = "Günlük Gelir")]
        [Required(ErrorMessage = "Gelir verisi gereklidir")]
        public List<RevenueDto> DailyRevenue { get; set; } = new List<RevenueDto>();
    }

    public class DashboardStatsDto
    {
        [Display(Name = "Toplam Otel")]
        [Required(ErrorMessage = "Toplam otel sayısı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Otel sayısı 0'dan büyük olmalıdır")]
        public int TotalHotels { get; set; }

        [Display(Name = "Toplam Rezervasyon")]
        [Required(ErrorMessage = "Toplam rezervasyon sayısı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Rezervasyon sayısı 0'dan büyük olmalıdır")]
        public int TotalReservations { get; set; }

        [Display(Name = "Toplam Müşteri")]
        [Required(ErrorMessage = "Toplam müşteri sayısı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Müşteri sayısı 0'dan büyük olmalıdır")]
        public int TotalGuests { get; set; }

        [Display(Name = "Toplam Gelir")]
        [Required(ErrorMessage = "Toplam gelir gereklidir")]
        [Range(0, double.MaxValue, ErrorMessage = "Gelir 0'dan büyük olmalıdır")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Beklemede Olan Rezervasyonlar")]
        [Required(ErrorMessage = "Beklemede olan rezervasyon sayısı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Sayı 0'dan büyük olmalıdır")]
        public int PendingReservations { get; set; }

        [Display(Name = "Ortalama Rating")]
        [Required(ErrorMessage = "Ortalama rating gereklidir")]
        [Range(0, 5, ErrorMessage = "Rating 0-5 arasında olmalıdır")]
        [DisplayFormat(DataFormatString = "{0:F1}", ApplyFormatInEditMode = true)]
        public double AverageRating { get; set; }
    }

    public class ReservationAdminDto
    {
        [Display(Name = "ID")]
        [Required(ErrorMessage = "Rezervasyon ID'si gereklidir")]
        public int Id { get; set; }

        [Display(Name = "Müşteri Adı")]
        [Required(ErrorMessage = "Müşteri adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Müşteri adı 3-100 karakter arasında olmalıdır")]
        public string GuestName { get; set; } = null!;

        [Display(Name = "Otel Adı")]
        [Required(ErrorMessage = "Otel adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Otel adı 3-100 karakter arasında olmalıdır")]
        public string HotelName { get; set; } = null!;

        [Display(Name = "Giriş Tarihi")]
        [Required(ErrorMessage = "Giriş tarihi gereklidir")]
        [DataType(DataType.DateTime)]
        public DateTime CheckInDate { get; set; }

        [Display(Name = "Çıkış Tarihi")]
        [Required(ErrorMessage = "Çıkış tarihi gereklidir")]
        [DataType(DataType.DateTime)]
        public DateTime CheckOutDate { get; set; }

        [Display(Name = "Toplam Fiyat")]
        [Required(ErrorMessage = "Toplam fiyat gereklidir")]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Durum")]
        [Required(ErrorMessage = "Durum gereklidir")]
        [StringLength(50, ErrorMessage = "Durum maksimum 50 karakter olmalıdır")]
        public string Status { get; set; } = null!;

        [Display(Name = "Oluşturma Tarihi")]
        [Required(ErrorMessage = "Oluşturma tarihi gereklidir")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }

    public class GuestSummaryDto
    {
        [Display(Name = "ID")]
        [Required(ErrorMessage = "Müşteri ID'si gereklidir")]
        public int Id { get; set; }

        [Display(Name = "Tam Adı")]
        [Required(ErrorMessage = "Tam ad gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tam ad 3-100 karakter arasında olmalıdır")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = null!;

        [Display(Name = "Telefon Numarası")]
        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string PhoneNumber { get; set; } = null!;

        [Display(Name = "Rezervasyon Sayısı")]
        [Required(ErrorMessage = "Rezervasyon sayısı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Sayı 0'dan büyük olmalıdır")]
        public int ReservationCount { get; set; }

        [Display(Name = "Katılma Tarihi")]
        [Required(ErrorMessage = "Katılma tarihi gereklidir")]
        [DataType(DataType.DateTime)]
        public DateTime JoinDate { get; set; }
    }

    public class RevenueDto
    {
        [Display(Name = "Tarih")]
        [Required(ErrorMessage = "Tarih gereklidir")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Display(Name = "Miktar")]
        [Required(ErrorMessage = "Gelir miktarı gereklidir")]
        [Range(0, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }

        [Display(Name = "Rezervasyon Sayısı")]
        [Required(ErrorMessage = "Rezervasyon sayısı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Sayı 0'dan büyük olmalıdır")]
        public int ReservationCount { get; set; }
    }
}