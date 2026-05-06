using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class AddOnServiceDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Hizmet Adı")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = null!;

        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }
        public bool IsActive   { get; set; }


        public class AddOnServiceDetailDto
        {
            [Display(Name = "ID")]
            public int Id { get; set; }

            [Required]
            [Display(Name = "Hizmet Adı")]
            public string Name { get; set; } = null!;

            [Required]
            [Display(Name = "Açıklama")]
            public string Description { get; set; } = null!;

            [Display(Name = "Fiyat")]
            public decimal Price { get; set; }

            [Display(Name = "Otel ID")]
            public int HotelId { get; set; }

            [Display(Name = "Otel Adı")]
            public string? HotelName { get; set; }

            [Display(Name = "Etkin")]
            public bool IsActive { get; set; }
        }

        public class CreateAddOnServiceDto
        {
            [Required(ErrorMessage = "Hizmet adı gereklidir")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Hizmet adı 3-100 karakter arasında olmalıdır")]
            [Display(Name = "Hizmet Adı")]
            public string Name { get; set; } = null!;

            [Required(ErrorMessage = "Açıklama gereklidir")]
            [StringLength(500, MinimumLength = 10, ErrorMessage = "Açıklama 10-500 karakter arasında olmalıdır")]
            [Display(Name = "Açıklama")]
            public string Description { get; set; } = null!;

            [Required(ErrorMessage = "Fiyat gereklidir")]
            [Range(0.01, 100000, ErrorMessage = "Geçerli bir fiyat giriniz")]
            [Display(Name = "Fiyat")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Otel ID gereklidir")]
            [Display(Name = "Otel ID")]
            public int HotelId { get; set; }
        }

        public class UpdateAddOnServiceDto
        {
            [Required(ErrorMessage = "Hizmet adı gereklidir")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Hizmet adı 3-100 karakter arasında olmalıdır")]
            [Display(Name = "Hizmet Adı")]
            public string Name { get; set; } = null!;

            [Required(ErrorMessage = "Açıklama gereklidir")]
            [StringLength(500, MinimumLength = 10, ErrorMessage = "Açıklama 10-500 karakter arasında olmalıdır")]
            [Display(Name = "Açıklama")]
            public string Description { get; set; } = null!;

            [Required(ErrorMessage = "Fiyat gereklidir")]
            [Range(0.01, 100000, ErrorMessage = "Geçerli bir fiyat giriniz")]
            [Display(Name = "Fiyat")]
            public decimal Price { get; set; }
        }
    }
}