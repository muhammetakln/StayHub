using Core.Concretes.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    // ═══════════════════════════════════════════════════════════════
    // PAYMENT DTOs - GÜNCELLENMIŞ
    // ═══════════════════════════════════════════════════════════════

    public class PaymentDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Sipariş Numarası")]
        public string OrderNumber { get; set; } = null!;

        [Display(Name = "Miktar")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Durum")]
        public string Status { get; set; } = null!;

        [Display(Name = "Ödeme Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Ödeme Yöntemi")]
        public string? PaymentMethod { get; set; }
        
    }

    public class PaymentDetailDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Sipariş Numarası")]
        public string OrderNumber { get; set; } = null!;

        [Display(Name = "Rezervasyon ID")]
        public int ReservationId { get; set; }

        [Display(Name = "Miktar")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Para Birimi")]
        public string Currency { get; set; } = null!;

        [Required]
        [Display(Name = "Durum")]
        public string Status { get; set; } = null!;

        [Display(Name = "Ödeme Yöntemi")]
        public string? PaymentMethod { get; set; }

        [Display(Name = "Transaction ID")]
        public string? TransactionId { get; set; }

        [Display(Name = "Ödeme Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Açıklamalar")]
        public string? Description { get; set; }
    }

    public class PaymentProcessDto
    {
        [Required(ErrorMessage = "Sipariş numarası gereklidir")]
        [Display(Name = "Sipariş Numarası")]
        public string OrderNumber { get; set; } = null!;

        [Required(ErrorMessage = "Miktar gereklidir")]
        [Range(0.01, 1000000, ErrorMessage = "Geçerli bir miktar giriniz")]
        [Display(Name = "Miktar")]
        public decimal Amount { get; set; }

        [Display(Name = "Para Birimi")]
        public string Currency { get; set; } = "TRY";

        // ✅ YENİ: PaymentMethod ekle
        [Display(Name = "Banka")]
        public string PaymentMethod { get; set; } = "garanti";

        [Required(ErrorMessage = "Kart numarası gereklidir")]
        [StringLength(19, MinimumLength = 13, ErrorMessage = "Geçerli bir kart numarası giriniz")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Kart numarası sadece rakamlardan oluşmalıdır")]
        [Display(Name = "Kart Numarası")]
        public string CardNumber { get; set; } = null!;

        [Required(ErrorMessage = "Kart sahibinin adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Kart sahibinin adı 3-100 karakter arasında olmalıdır")]
        [Display(Name = "Kart Sahibinin Adı")]
        public string CardHolder { get; set; } = null!;

        [Required(ErrorMessage = "Son kullanma tarihi gereklidir")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Son kullanma tarihi MM/YY formatında olmalıdır")]
        [RegularExpression(@"^(0[1-9]|1[0-2])/\d{2}$", ErrorMessage = "Son kullanma tarihi MM/YY formatında olmalıdır")]
        [Display(Name = "Son Kullanma Tarihi")]
        public string ExpiryDate { get; set; } = null!;

        [Required(ErrorMessage = "CVV gereklidir")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV 3-4 karakter arasında olmalıdır")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV sadece rakamlardan oluşmalıdır")]
        [Display(Name = "CVV")]
        public string CVV { get; set; } = null!;

        [Display(Name = "Açıklamalar")]
        public string? Description { get; set; }
    }

    public class PaymentResponseDto
    {
        [Display(Name = "Başarı")]
        public bool Success { get; set; }

        [Display(Name = "Mesaj")]
        public string Message { get; set; } = null!;

        [Display(Name = "Transaction ID")]
        public string? TransactionId { get; set; }

        [Display(Name = "Ödeme ID")]
        public int? PaymentId { get; set; }

        [Display(Name = "Ödeme Bilgileri")]
        public PaymentDto? Payment { get; set; }

        [Display(Name = "Hata Detayları")]
        public string? ErrorDetails { get; set; }
    }

    public class RefundDto
    {
        [Required(ErrorMessage = "Ödeme ID gereklidir")]
        [Display(Name = "Ödeme ID")]
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "İade sebebi gereklidir")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "İade sebebi 10-500 karakter arasında olmalıdır")]
        [Display(Name = "İade Sebebi")]
        public string RefundReason { get; set; } = null!;

        [Display(Name = "İade Miktarı")]
        [Range(0.01, 1000000, ErrorMessage = "Geçerli bir miktar giriniz")]
        public decimal? RefundAmount { get; set; }
    }
    public class PaymentRequestDto
    {
        // Kredi Kartı Bilgileri (Veritabanına ASLA kaydedilmez, sadece RAM'de yaşar)
        public string CardHolderName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string ExpireMonth { get; set; } = string.Empty;
        public string ExpireYear { get; set; } = string.Empty;
        public string Cvc { get; set; } = string.Empty;

        // Sipariş Bilgileri
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "TRY";
        public string BuyerEmail { get; set; } = string.Empty;
        public string BuyerIp { get; set; } = string.Empty;

    }
    public class PaymentResultDto
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = string.Empty; // Bankanın verdiği dekont/işlem numarası
        public string ErrorMessage { get; set; } = string.Empty;
    }
}