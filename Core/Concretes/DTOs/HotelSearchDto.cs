using System.ComponentModel.DataAnnotations;
/// <summary>
/// Hotel Search DTO
/// Otel araması için
/// </summary>
   
namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Hotel Search DTO
    /// Otel araması için
    /// </summary>
    public class HotelSearchDto
    {
        [Display(Name = "Check-in Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? CheckInDate { get; set; }

        [Display(Name = "Check-out Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? CheckOutDate { get; set; }

        [StringLength(100, ErrorMessage = "Şehir adı maksimum 100 karakter olmalıdır")]
        [Display(Name = "Şehir")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Ülke adı maksimum 100 karakter olmalıdır")]
        [Display(Name = "Ülke")]
        public string? Country { get; set; }

        [Range(1, 5, ErrorMessage = "Yıldız sayısı 1-5 arasında olmalıdır")]
        [Display(Name = "Yıldız Sayısı")]
        public int? StarRating { get; set; }

        [Range(0, 1000000, ErrorMessage = "Minimum fiyat 0-1000000 arasında olmalıdır")]
        [Display(Name = "Minimum Fiyat")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? MinPrice { get; set; }

        [Range(0, 1000000, ErrorMessage = "Maksimum fiyat 0-1000000 arasında olmalıdır")]
        [Display(Name = "Maksimum Fiyat")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? MaxPrice { get; set; }

        [Range(1, 10, ErrorMessage = "Misafir sayısı 1-10 arasında olmalıdır")]
        [Display(Name = "Misafir Sayısı")]
        public int? GuestCount { get; set; }

        [StringLength(50, ErrorMessage = "Sıralama maksimum 50 karakter olmalıdır")]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; }

        [Display(Name = "Azalan Sıralama")]
        public bool SortDescending { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası 1'den büyük olmalıdır")]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Sayfa boyutu 1-100 arasında olmalıdır")]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Room Availability DTO
    /// Oda müsaitlik kontrolü
    /// </summary>
    public class RoomAvailabilityDto
    {
        [Required(ErrorMessage = "Hotel ID gereklidir")]
        [Display(Name = "Hotel ID")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir Hotel ID giriniz")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "Check-in tarihi gereklidir")]
        [Display(Name = "Check-in")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out tarihi gereklidir")]
        [Display(Name = "Check-out")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 10, ErrorMessage = "Misafir sayısı 1-10 arasında olmalıdır")]
        [Display(Name = "Misafir Sayısı")]
        public int GuestCount { get; set; } = 1;

        [StringLength(50, ErrorMessage = "Oda tipi maksimum 50 karakter olmalıdır")]
        [Display(Name = "Oda Tipi")]
        public string? RoomType { get; set; }

        [Range(0, 1000000, ErrorMessage = "Maksimum fiyat 0-1000000 arasında olmalıdır")]
        [Display(Name = "Maksimum Fiyat")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? MaxPrice { get; set; }
    }

    /// <summary>
    /// Reservation Filter DTO
    /// Rezervasyon filtreleme
    /// </summary>
    public class ReservationFilterDto
    {
        [StringLength(50, ErrorMessage = "Durum maksimum 50 karakter olmalıdır")]
        [Display(Name = "Durum")]
        public string? Status { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Range(0, 1000000, ErrorMessage = "Minimum fiyat 0-1000000 arasında olmalıdır")]
        [Display(Name = "Minimum Fiyat")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? MinPrice { get; set; }

        [Range(0, 1000000, ErrorMessage = "Maksimum fiyat 0-1000000 arasında olmalıdır")]
        [Display(Name = "Maksimum Fiyat")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? MaxPrice { get; set; }

        [StringLength(100, ErrorMessage = "Guest adı maksimum 100 karakter olmalıdır")]
        [Display(Name = "Guest Adı")]
        public string? GuestName { get; set; }

        [StringLength(100, ErrorMessage = "Hotel adı maksimum 100 karakter olmalıdır")]
        [Display(Name = "Hotel Adı")]
        public string? HotelName { get; set; }

        [StringLength(50, ErrorMessage = "Sıralama maksimum 50 karakter olmalıdır")]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; } = "CreatedAt";

        [Display(Name = "Azalan Sıralama")]
        public bool SortDescending { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası 1'den büyük olmalıdır")]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Sayfa boyutu 1-100 arasında olmalıdır")]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Review Filter DTO
    /// Yorum filtreleme
    /// </summary>
    public class ReviewFilterDto
    {
        [Range(1, 5, ErrorMessage = "Minimum rating 1-5 arasında olmalıdır")]
        [Display(Name = "Minimum Rating")]
        public int? MinRating { get; set; }

        [Range(1, 5, ErrorMessage = "Maksimum rating 1-5 arasında olmalıdır")]
        [Display(Name = "Maksimum Rating")]
        public int? MaxRating { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Yayınlanan")]
        public bool? IsPublished { get; set; }

        [Display(Name = "Yanıtlanan")]
        public bool? IsReplied { get; set; }

        [StringLength(100, ErrorMessage = "Hotel adı maksimum 100 karakter olmalıdır")]
        [Display(Name = "Hotel Adı")]
        public string? HotelName { get; set; }

        [StringLength(100, ErrorMessage = "Guest adı maksimum 100 karakter olmalıdır")]
        [Display(Name = "Guest Adı")]
        public string? GuestName { get; set; }

        [StringLength(50, ErrorMessage = "Sıralama maksimum 50 karakter olmalıdır")]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; } = "CreatedAt";

        [Display(Name = "Azalan Sıralama")]
        public bool SortDescending { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası 1'den büyük olmalıdır")]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Sayfa boyutu 1-100 arasında olmalıdır")]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Room Search DTO
    /// Oda araması
    /// </summary>
    public class RoomSearchDto
    {
        [Required(ErrorMessage = "Hotel ID gereklidir")]
        [Display(Name = "Hotel ID")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir Hotel ID giriniz")]
        public int HotelId { get; set; }

        [StringLength(50, ErrorMessage = "Oda tipi maksimum 50 karakter olmalıdır")]
        [Display(Name = "Oda Tipi")]
        public string? RoomType { get; set; }

        [Range(1, 10, ErrorMessage = "Minimum kapasite 1-10 arasında olmalıdır")]
        [Display(Name = "Minimum Kapasite")]
        public int? MinCapacity { get; set; }

        [Range(1, 10, ErrorMessage = "Maksimum kapasite 1-10 arasında olmalıdır")]
        [Display(Name = "Maksimum Kapasite")]
        public int? MaxCapacity { get; set; }

        [Range(0, 1000000, ErrorMessage = "Minimum fiyat 0-1000000 arasında olmalıdır")]
        [Display(Name = "Minimum Fiyat")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? MinPrice { get; set; }

        [Range(0, 1000000, ErrorMessage = "Maksimum fiyat 0-1000000 arasında olmalıdır")]
        [Display(Name = "Maksimum Fiyat")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? MaxPrice { get; set; }

        [StringLength(50, ErrorMessage = "Sıralama maksimum 50 karakter olmalıdır")]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; }

        [Display(Name = "Azalan Sıralama")]
        public bool SortDescending { get; set; } = false;

        [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası 1'den büyük olmalıdır")]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Sayfa boyutu 1-100 arasında olmalıdır")]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Guest Search DTO
    /// Guest araması (Admin için)
    /// </summary>
    public class GuestSearchDto
    {
        [StringLength(100, ErrorMessage = "Ad maksimum 100 karakter olmalıdır")]
        [Display(Name = "Ad")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "Soyad maksimum 100 karakter olmalıdır")]
        [Display(Name = "Soyad")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [StringLength(100, ErrorMessage = "Email maksimum 100 karakter olmalıdır")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(100, ErrorMessage = "Şehir adı maksimum 100 karakter olmalıdır")]
        [Display(Name = "Şehir")]
        public string? City { get; set; }

        [Display(Name = "Aktif")]
        public bool? IsActive { get; set; }

        [Display(Name = "Email Doğrulandı")]
        public bool? IsEmailVerified { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [StringLength(50, ErrorMessage = "Sıralama maksimum 50 karakter olmalıdır")]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; } = "CreatedAt";

        [Display(Name = "Azalan Sıralama")]
        public bool SortDescending { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası 1'den büyük olmalıdır")]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Sayfa boyutu 1-100 arasında olmalıdır")]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }
}

/// <summary>
/// Room Availability DTO
/// Oda müsaitlik kontrolü
/// </summary>
public class RoomAvailabilityDto
    {
        [Required(ErrorMessage = "Hotel ID gereklidir")]
        [Display(Name = "Hotel ID")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "Check-in tarihi gereklidir")]
        [Display(Name = "Check-in")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out tarihi gereklidir")]
        [Display(Name = "Check-out")]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 10)]
        [Display(Name = "Misafir Sayısı")]
        public int GuestCount { get; set; } = 1;

        [StringLength(50)]
        [Display(Name = "Oda Tipi")]
        public string? RoomType { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Maksimum Fiyat")]
        public decimal? MaxPrice { get; set; }
    }

    /// <summary>
    /// Reservation Filter DTO
    /// Rezervasyon filtreleme
    /// </summary>
    public class ReservationFilterDto
    {
        [StringLength(50)]
        [Display(Name = "Durum")]
        public string? Status { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? ToDate { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Minimum Fiyat")]
        public decimal? MinPrice { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Maksimum Fiyat")]
        public decimal? MaxPrice { get; set; }

        [StringLength(100)]
        [Display(Name = "Guest Adı")]
        public string? GuestName { get; set; }

        [StringLength(100)]
        [Display(Name = "Hotel Adı")]
        public string? HotelName { get; set; }

        [StringLength(50)]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; } = "CreatedAt";

        [Display(Name = "Azalan")]
        public bool SortDescending { get; set; } = true;

        [Range(1, int.MaxValue)]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Review Filter DTO
    /// Yorum filtreleme
    /// </summary>
    public class ReviewFilterDto
    {
        [Range(1, 5)]
        [Display(Name = "Minimum Rating")]
        public int? MinRating { get; set; }

        [Range(1, 5)]
        [Display(Name = "Maksimum Rating")]
        public int? MaxRating { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Yayınlanan")]
        public bool? IsPublished { get; set; }

        [Display(Name = "Yanıtlanan")]
        public bool? IsReplied { get; set; }

        [StringLength(100)]
        [Display(Name = "Hotel Adı")]
        public string? HotelName { get; set; }

        [StringLength(100)]
        [Display(Name = "Guest Adı")]
        public string? GuestName { get; set; }

        [StringLength(50)]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; } = "CreatedAt";

        [Display(Name = "Azalan")]
        public bool SortDescending { get; set; } = true;

        [Range(1, int.MaxValue)]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Room Search DTO
    /// Oda araması
    /// </summary>
    public class RoomSearchDto
    {
        [Required(ErrorMessage = "Hotel ID gereklidir")]
        [Display(Name = "Hotel ID")]
        public int HotelId { get; set; }

        [StringLength(50)]
        [Display(Name = "Oda Tipi")]
        public string? RoomType { get; set; }

        [Range(1, 10)]
        [Display(Name = "Minimum Kapasite")]
        public int? MinCapacity { get; set; }

        [Range(1, 10)]
        [Display(Name = "Maksimum Kapasite")]
        public int? MaxCapacity { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Minimum Fiyat")]
        public decimal? MinPrice { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Maksimum Fiyat")]
        public decimal? MaxPrice { get; set; }

        [StringLength(50)]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; }

        [Display(Name = "Azalan")]
        public bool SortDescending { get; set; } = false;

        [Range(1, int.MaxValue)]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Guest Search DTO
    /// Guest araması (Admin için)
    /// </summary>
    public class GuestSearchDto
    {
        [StringLength(100)]
        [Display(Name = "Ad")]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [Display(Name = "Soyad")]
        public string? LastName { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(100)]
        [Display(Name = "Şehir")]
        public string? City { get; set; }

        [Display(Name = "Aktif")]
        public bool? IsActive { get; set; }

        [Display(Name = "Email Doğrulandı")]
        public bool? IsEmailVerified { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? ToDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Sıralama")]
        public string? SortBy { get; set; } = "CreatedAt";

        [Display(Name = "Azalan")]
        public bool SortDescending { get; set; } = true;

        [Range(1, int.MaxValue)]
        [Display(Name = "Sayfa")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; } = 10;
    }
