using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Reservation Status Enum DTO
    /// </summary>
    public enum ReservationStatusDto
    {
        [Display(Name = "Beklemede")]
        Pending = 0,

        [Display(Name = "Onaylandı")]
        Confirmed = 1,

        [Display(Name = "Check-in Yapıldı")]
        CheckedIn = 2,

        [Display(Name = "Check-out Yapıldı")]
        CheckedOut = 3,

        [Display(Name = "İptal Edildi")]
        Cancelled = 4,

        [Display(Name = "Tamamlandı")]
        Completed = 5
    }

    /// <summary>
    /// Room Status Enum DTO
    /// </summary>
    public enum RoomStatusDto
    {
        [Display(Name = "Müsait")]
        Available = 0,

        [Display(Name = "Dolu")]
        Occupied = 1,

        [Display(Name = "Bakım")]
        Maintenance = 2,

        [Display(Name = "Temizlenmesi Gerekli")]
        NeedsCleaning = 3,

        [Display(Name = "Kullanılamaz")]
        Unavailable = 4
    }

    /// <summary>
    /// Payment Status Enum DTO
    /// </summary>
    public enum PaymentStatusDto
    {
        [Display(Name = "Beklemede")]
        Pending = 0,

        [Display(Name = "İşleniyor")]
        Processing = 1,

        [Display(Name = "Başarılı")]
        Completed = 2,

        [Display(Name = "Başarısız")]
        Failed = 3,

        [Display(Name = "İade Edildi")]
        Refunded = 4,

        [Display(Name = "Kısmen İade")]
        PartiallyRefunded = 5
    }

    /// <summary>
    /// Payment Method Enum DTO
    /// </summary>
    public enum PaymentMethodDto
    {
        [Display(Name = "Kredi Kartı")]
        CreditCard = 0,

        [Display(Name = "Debit Kartı")]
        DebitCard = 1,

        [Display(Name = "Banka Havalesi")]
        BankTransfer = 2,

        [Display(Name = "E-Cüzdan")]
        EWallet = 3,

        [Display(Name = "Kapıda Ödeme")]
        PayAtDoor = 4
    }

    /// <summary>
    /// Room Type Enum DTO
    /// </summary>
    public enum RoomTypeDto
    {
        [Display(Name = "Tek Kişilik")]
        Single = 0,

        [Display(Name = "Çift Kişilik")]
        Double = 1,

        [Display(Name = "Üç Kişilik")]
        Triple = 2,

        [Display(Name = "Dört Kişilik")]
        Quad = 3,

        [Display(Name = "Suite")]
        Suite = 4,

        [Display(Name = "Deluxe")]
        Deluxe = 5,

        [Display(Name = "Presidential")]
        Presidential = 6
    }

    /// <summary>
    /// Review Rating Enum DTO
    /// </summary>
    public enum ReviewRatingDto
    {
        [Display(Name = "Çok Kötü")]
        VeryPoor = 1,

        [Display(Name = "Kötü")]
        Poor = 2,

        [Display(Name = "Orta")]
        Average = 3,

        [Display(Name = "İyi")]
        Good = 4,

        [Display(Name = "Çok İyi")]
        Excellent = 5
    }

    /// <summary>
    /// Sort Order Enum DTO
    /// </summary>
    public enum SortOrderDto
    {
        [Display(Name = "Artan")]
        Ascending = 0,

        [Display(Name = "Azalan")]
        Descending = 1
    }

    /// <summary>
    /// User Role Enum DTO
    /// </summary>
    public enum UserRoleDto
    {
        [Display(Name = "Admin")]
        Admin = 0,

        [Display(Name = "Kullanıcı")]
        User = 1,

        [Display(Name = "Otel Sahibi")]
        HotelOwner = 2,

        [Display(Name = "Personel")]
        Staff = 3
    }

    /// <summary>
    /// Permission Category Enum DTO
    /// </summary>
    public enum PermissionCategoryDto
    {
        [Display(Name = "Hotel")]
        Hotel = 0,

        [Display(Name = "Room")]
        Room = 1,

        [Display(Name = "Reservation")]
        Reservation = 2,

        [Display(Name = "Payment")]
        Payment = 3,

        [Display(Name = "Review")]
        Review = 4,

        [Display(Name = "User")]
        User = 5,

        [Display(Name = "Report")]
        Report = 6,

        [Display(Name = "Settings")]
        Settings = 7
    }

    /// <summary>
    /// Refund Status Enum DTO
    /// </summary>
    public enum RefundStatusDto
    {
        [Display(Name = "Başlatıldı")]
        Initiated = 0,

        [Display(Name = "İşleniyor")]
        Processing = 1,

        [Display(Name = "Tamamlandı")]
        Completed = 2,

        [Display(Name = "Başarısız")]
        Failed = 3,

        [Display(Name = "İptal Edildi")]
        Cancelled = 4
    }

    /// <summary>
    /// Cancellation Reason Enum DTO
    /// </summary>
    public enum CancellationReasonDto
    {
        [Display(Name = "Diğer")]
        Other = 0,

        [Display(Name = "Planlama Değişti")]
        ChangedPlans = 1,

        [Display(Name = "Finansal Nedenler")]
        FinancialReasons = 2,

        [Display(Name = "Sağlık Sorunları")]
        HealthIssues = 3,

        [Display(Name = "Düşük Fiyat Buldum")]
        FoundBetterPrice = 4,

        [Display(Name = "Otel Standartları Yetersiz")]
        PoorHotelQuality = 5
    }
}