using System.ComponentModel.DataAnnotations;

public class ApiResponseDto
{
    [Display(Name = "Başarı")]
    public bool Success { get; set; }

    [Display(Name = "Mesaj")]
    public string? Message { get; set; }

    [Display(Name = "Hata Detayları")]
    public List<string>? Errors { get; set; } = new List<string>();

    [Display(Name = "Timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;


    /// <summary>
    /// Error Response
    /// </summary>
    public class ErrorResponseDto
    {
        [Display(Name = "Hata Kodu")]
        public string? Code { get; set; }

        [Display(Name = "Mesaj")]
        public string Message { get; set; } = null!;

        [Display(Name = "Detaylar")]
        public Dictionary<string, List<string>>? Details { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Display(Name = "Trace ID")]
        public string? TraceId { get; set; }
    }

    /// <summary>
    /// Validation Error Response
    /// </summary>
    public class ValidationErrorResponseDto
    {
        [Display(Name = "Mesaj")]
        public string Message { get; set; } = "Validasyon hatası";

        [Display(Name = "Hata Detayları")]
        public Dictionary<string, List<string>> Errors { get; set; } = new Dictionary<string, List<string>>();

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Unauthorized Response
    /// </summary>
    public class UnauthorizedResponseDto
    {
        [Display(Name = "Mesaj")]
        public string Message { get; set; } = "Yetkilendirme başarısız";

        [Display(Name = "Detay")]
        public string? Detail { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Forbidden Response
    /// </summary>
    public class ForbiddenResponseDto
    {
        [Display(Name = "Mesaj")]
        public string Message { get; set; } = "Erişim yasak";

        [Display(Name = "Detay")]
        public string? Detail { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Not Found Response
    /// </summary>
    public class NotFoundResponseDto
    {
        [Display(Name = "Mesaj")]
        public string Message { get; set; } = "Kayıt bulunamadı";

        [Display(Name = "Detay")]
        public string? Detail { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Conflict Response (409)
    /// </summary>
    public class ConflictResponseDto
    {
        [Display(Name = "Mesaj")]
        public string Message { get; set; } = "Çakışma oluştu";

        [Display(Name = "Detay")]
        public string? Detail { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Server Error Response
    /// </summary>
    public class ServerErrorResponseDto
    {
        [Display(Name = "Mesaj")]
        public string Message { get; set; } = "Sunucu hatası";

        [Display(Name = "Detay")]
        public string? Detail { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Display(Name = "Trace ID")]
        public string? TraceId { get; set; }
    }


}