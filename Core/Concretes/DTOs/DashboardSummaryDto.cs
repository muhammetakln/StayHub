using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Dashboard Summary DTO
    /// Admin dashboard için
    /// </summary>
    public class DashboardSummaryDto
    {
        [Display(Name = "Toplam Misafir")]
        public int TotalGuests { get; set; }

        [Display(Name = "Toplam Otel")]
        public int TotalHotels { get; set; }

        [Display(Name = "Toplam Oda")]
        public int TotalRooms { get; set; }

        [Display(Name = "Aktif Rezervasyonlar")]
        public int ActiveReservations { get; set; }

        [Display(Name = "Beklemede Ödeme")]
        public int PendingPayments { get; set; }

        [Display(Name = "Bu Ayın Gelir")]
        public decimal MonthlyRevenue { get; set; }

        [Display(Name = "Ortalama İşgal Oranı")]
        public double AverageOccupancyRate { get; set; }

        [Display(Name = "Ortalama Rating")]
        public double AverageRating { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Revenue Statistics DTO
    /// Gelir istatistikleri
    /// </summary>
    public class RevenueStatisticsDto
    {
        [Display(Name = "Dönem")]
        public string Period { get; set; } = null!;

        [Display(Name = "Toplam Gelir")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Rezervasyon Gelirleri")]
        public decimal ReservationRevenue { get; set; }

        [Display(Name = "Ek Hizmet Gelirleri")]
        public decimal AddOnServiceRevenue { get; set; }

        [Display(Name = "Rezervasyon Sayısı")]
        public int ReservationCount { get; set; }

        [Display(Name = "Ortalama Rezervasyon Değeri")]
        public decimal AverageReservationValue { get; set; }

        [Display(Name = "Ödenen Ödeme")]
        public decimal CompletedPayments { get; set; }

        [Display(Name = "Beklemede Ödeme")]
        public decimal PendingPayments { get; set; }

        [Display(Name = "İade Edilen")]
        public decimal RefundedAmount { get; set; }
    }

    /// <summary>
    /// Occupancy Statistics DTO
    /// İşgal istatistikleri
    /// </summary>
    public class OccupancyStatisticsDto
    {
        [Display(Name = "Dönem")]
        public string Period { get; set; } = null!;

        [Display(Name = "Toplam Oda")]
        public int TotalRooms { get; set; }

        [Display(Name = "Dolu Odalar")]
        public int OccupiedRooms { get; set; }

        [Display(Name = "Müsait Odalar")]
        public int AvailableRooms { get; set; }

        [Display(Name = "Bakımdaki Odalar")]
        public int MaintenanceRooms { get; set; }

        [Display(Name = "İşgal Oranı")]
        public double OccupancyRate { get; set; }

        [Display(Name = "Ortalama Oda Fiyatı")]
        public decimal AverageRoomPrice { get; set; }

        [Display(Name = "Oda Gecelik Sayısı")]
        public int RoomNights { get; set; }
    }

    /// <summary>
    /// Reservation Report DTO
    /// Rezervasyon raporu
    /// </summary>
    public class ReservationReportDto
    {
        [Display(Name = "Rapor Tarihi")]
        public string ReportDate { get; set; } = null!;

        [Display(Name = "Toplam Rezervasyon")]
        public int TotalReservations { get; set; }

        [Display(Name = "Onaylanan")]
        public int ConfirmedReservations { get; set; }

        [Display(Name = "Beklemede")]
        public int PendingReservations { get; set; }

        [Display(Name = "Check-in")]
        public int CheckedInReservations { get; set; }

        [Display(Name = "Check-out")]
        public int CheckedOutReservations { get; set; }

        [Display(Name = "İptal Edilen")]
        public int CancelledReservations { get; set; }

        [Display(Name = "Ortalama Gece Sayısı")]
        public double AverageNightCount { get; set; }

        [Display(Name = "Ortalama Rezervasyon Değeri")]
        public decimal AverageReservationValue { get; set; }

        [Display(Name = "Toplam Gelir")]
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// Guest Statistics DTO
    /// Misafir istatistikleri
    /// </summary>
    public class GuestStatisticsDto
    {
        [Display(Name = "Toplam Misafir")]
        public int TotalGuests { get; set; }

        [Display(Name = "Bu Ay Kaydolan")]
        public int NewGuestsThisMonth { get; set; }

        [Display(Name = "Etkin Misafirler")]
        public int ActiveGuests { get; set; }

        [Display(Name = "Kilitli Hesaplar")]
        public int LockedAccounts { get; set; }

        [Display(Name = "Email Doğrulanan")]
        public int EmailVerifiedGuests { get; set; }

        [Display(Name = "Ortalama Rezervasyon Sayısı")]
        public double AverageReservationCount { get; set; }

        [Display(Name = "Üst Harcayan Misafir")]
        public string? TopSpenderName { get; set; }

        [Display(Name = "En Çok Rezervasyon")]
        public int MostReservationsCount { get; set; }
    }

    /// <summary>
    /// Review Statistics DTO
    /// Yorum istatistikleri
    /// </summary>
    public class ReviewStatisticsDto
    {
        [Display(Name = "Toplam Yorum")]
        public int TotalReviews { get; set; }

        [Display(Name = "Yayınlanan")]
        public int PublishedReviews { get; set; }

        [Display(Name = "Yanıtlanan")]
        public int RepliedReviews { get; set; }

        [Display(Name = "Ortalama Rating")]
        public double AverageRating { get; set; }

        [Display(Name = "5 Yıldız")]
        public int FiveStarCount { get; set; }

        [Display(Name = "4 Yıldız")]
        public int FourStarCount { get; set; }

        [Display(Name = "3 Yıldız")]
        public int ThreeStarCount { get; set; }

        [Display(Name = "2 Yıldız")]
        public int TwoStarCount { get; set; }

        [Display(Name = "1 Yıldız")]
        public int OneStarCount { get; set; }

        [Display(Name = "Ortalama Yardımcı Sayısı")]
        public double AverageHelpfulCount { get; set; }
    }

    /// <summary>
    /// Hotel Performance DTO
    /// Otel performansı
    /// </summary>
    public class HotelPerformanceDto
    {
        [Display(Name = "Otel ID")]
        public int HotelId { get; set; }

        [Display(Name = "Otel Adı")]
        public string HotelName { get; set; } = null!;

        [Display(Name = "Yıldız")]
        public int StarRating { get; set; }

        [Display(Name = "Toplam Oda")]
        public int TotalRooms { get; set; }

        [Display(Name = "İşgal Oranı")]
        public double OccupancyRate { get; set; }

        [Display(Name = "Ortalama Rating")]
        public double AverageRating { get; set; }

        [Display(Name = "Yorum Sayısı")]
        public int ReviewCount { get; set; }

        [Display(Name = "Bu Ayın Gelir")]
        public decimal MonthlyRevenue { get; set; }

        [Display(Name = "Ortalama Oda Fiyatı")]
        public decimal AverageRoomPrice { get; set; }

        [Display(Name = "Toplam Rezervasyon")]
        public int TotalReservations { get; set; }
    }

    /// <summary>
    /// Payment Report DTO
    /// Ödeme raporu
    /// </summary>
    public class PaymentReportDto
    {
        [Display(Name = "Rapor Tarihi")]
        public string ReportDate { get; set; } = null!;

        [Display(Name = "Toplam İşlem")]
        public int TotalTransactions { get; set; }

        [Display(Name = "Başarılı")]
        public int SuccessfulTransactions { get; set; }

        [Display(Name = "Başarısız")]
        public int FailedTransactions { get; set; }

        [Display(Name = "Beklemede")]
        public int PendingTransactions { get; set; }

        [Display(Name = "Toplam Tutar")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Kredi Kartı")]
        public decimal CreditCardAmount { get; set; }

        [Display(Name = "Banka Havalesi")]
        public decimal BankTransferAmount { get; set; }

        [Display(Name = "İade Edilen")]
        public decimal RefundedAmount { get; set; }

        [Display(Name = "Ortalama İşlem Tutarı")]
        public decimal AverageTransactionAmount { get; set; }
    }
}