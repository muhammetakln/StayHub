using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class RoomDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Oda Adı")]
        public string Name { get; set; } = null!;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Kapasite")]
        public int Capacity { get; set; }

        [Required]
        [Display(Name = "Alan (m²)")]
        public int Size { get; set; }

        [Required]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;
    }

    public class RoomDetailDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Oda Numarası")]
        public string RoomNumber { get; set; } = null!;

        [Required]
        [Display(Name = "Oda Tipi")]
        public string Type { get; set; } = null!;

        [Display(Name = "Kapasite")]
        public int Capacity { get; set; }

        [Display(Name = "Gecesi Fiyat")]
        public decimal PricePerNight { get; set; }

        [Required]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = null!;

        [Display(Name = "Resimler")]
        public IEnumerable<string> Images { get; set; } = [];
    }

    public class RoomCardDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Oda Numarası")]
        public string RoomNumber { get; set; } = null!;

        [Required]
        [Display(Name = "Oda Tipi")]
        public string Type { get; set; } = null!;

        [Display(Name = "Kapasite")]
        public int Capacity { get; set; }

        [Display(Name = "Gecesi Fiyat")]
        public decimal PricePerNight { get; set; }

        [Display(Name = "Kapak Resmi")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Müsait")]
        public bool IsAvailable { get; set; }
    }

    public class CreateRoomDto
    {
        [Required(ErrorMessage = "Oda adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Oda adı 3-100 karakter arasında olmalıdır")]
        [Display(Name = "Oda Adı")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Açıklama maksimum 500 karakter olmalıdır")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Kapasite gereklidir")]
        [Range(1, 10, ErrorMessage = "Kapasite 1-10 arasında olmalıdır")]
        [Display(Name = "Kapasite")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Alan gereklidir")]
        [Range(10, 500, ErrorMessage = "Alan 10-500 m² arasında olmalıdır")]
        [Display(Name = "Alan (m²)")]
        public int Size { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir")]
        [Range(0.01, 10000, ErrorMessage = "Fiyat 0.01-10000 arasında olmalıdır")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }

    public class UpdateRoomDto
    {
        [Required(ErrorMessage = "Oda adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Oda adı 3-100 karakter arasında olmalıdır")]
        [Display(Name = "Oda Adı")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Açıklama maksimum 500 karakter olmalıdır")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Kapasite gereklidir")]
        [Range(1, 10, ErrorMessage = "Kapasite 1-10 arasında olmalıdır")]
        [Display(Name = "Kapasite")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Alan gereklidir")]
        [Range(10, 500, ErrorMessage = "Alan 10-500 m² arasında olmalıdır")]
        [Display(Name = "Alan (m²)")]
        public int Size { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir")]
        [Range(0.01, 10000, ErrorMessage = "Fiyat 0.01-10000 arasında olmalıdır")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }

    public class RoomImageDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Resim URL")]
        public string ImageUrl { get; set; } = null!;

        [Display(Name = "Oda ID")]
        public int RoomId { get; set; }
    }

    public class CreateRoomImageDto
    {
        [Required(ErrorMessage = "Resim URL gereklidir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        [Display(Name = "Resim URL")]
        public string ImageUrl { get; set; } = null!;

        [Required(ErrorMessage = "Oda ID gereklidir")]
        [Display(Name = "Oda ID")]
        public int RoomId { get; set; }
    }
}