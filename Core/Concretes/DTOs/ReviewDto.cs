using Microsoft.EntityFrameworkCore.Query.Internal;
using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class ReviewDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }
        public string Title { get; set; }=string.Empty;
        public string Content { get; set; }=string.Empty;

        [Required]
        [Display(Name = "Misafir Adı")]
        public string GuestName { get; set; } = "Misafir";

        [Display(Name = "Puan")]
        public int Rating { get; set; }

        [Required]
        [Display(Name = "Yorum")]
        public string Comment { get; set; } = null!;

        [Display(Name = "Oluşturulma Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }
    public class ReviewListDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Misafir Adı")]
        public string GuestName { get; set; } = null!;

        [Display(Name = "Başlık")]
        public string Title { get; set; } = null!;

        [Display(Name = "Puan")]
        public int Rating { get; set; }

        [Required]
        [Display(Name = "Yorum")]
        public string Content { get; set; } = null!;

        [Display(Name = "Oluşturulma Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }


    public class ReviewDetailDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Otel ID")]
        public int HotelId { get; set; }

        [Required]
        [Display(Name = "Otel Adı")]
        public string HotelName { get; set; } = null!;


        [Required]
        [Display(Name = "Misafir Adı")]
        public string Name { get; set; } = null!;

        [Display(Name = "Puan")]
        public int Rating { get; set; }

        [Required]
        [Display(Name = "Yorum")]
        public string Comment { get; set; } = null!;

        [Display(Name = "Oluşturulma Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Başlık 5-200 karakter arasında olmalıdır")]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Yorum gereklidir")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Yorum 10-1000 karakter arasında olmalıdır")]
        [Display(Name = "Yorum")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "Puan gereklidir")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        [Display(Name = "Puan (1-5)")]
        public int Rating { get; set; }
        public int HotelId { get; set; }
    }

   public class UpdateReviewDto
    {
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Başlık 5-200 karakter arasında olmalıdır")]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Yorum gereklidir")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Yorum 10-1000 karakter arasında olmalıdır")]
        [Display(Name = "Yorum")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "Puan gereklidir")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        [Display(Name = "Puan (1-5)")]
        public int Rating { get; set; }
    }
}