using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class AmenityDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Olanak Adı")]
        public string Name { get; set; } = null!;

        [Display(Name = "Ikon URL")]
        public string? IconUrl { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
    }

    public class CreateAmenityDto
    {
        [Required(ErrorMessage = "Olanak adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Olanak adı 3-100 karakter arasında olmalıdır")]
        [Display(Name = "Olanak Adı")]
        public string Name { get; set; } = null!;

        [Display(Name = "Ikon URL")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string? IconUrl { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama 500 karakteri geçemez")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Otel ID gereklidir")]
        [Display(Name = "Otel ID")]
        public int HotelId { get; set; }
    }

    public class UpdateAmenityDto
    {
        [Required(ErrorMessage = "Olanak adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Olanak adı 3-100 karakter arasında olmalıdır")]
        [Display(Name = "Olanak Adı")]
        public string Name { get; set; } = null!;

        [Display(Name = "Ikon URL")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string? IconUrl { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama 500 karakteri geçemez")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
    }
}