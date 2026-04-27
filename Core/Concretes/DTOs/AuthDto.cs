using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Login için kullanılan DTO
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Email adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email Adresi")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage ="Kullamıcı tipini seçiniz ")]
        [Display(Name ="Giriş Türü")]
        public string UserType { get; set; } = null!;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakterden oluşmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = null!;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// Kayıt için kullanılan DTO
    /// </summary>
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email Adresi")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon Numarası")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Kimlik numarası gereklidir")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Kimlik numarası 11 haneli olmalıdır")]
        [Display(Name = "Kimlik Numarası")]
        public string IdentificationNumber { get; set; } = null!;

        [Required(ErrorMessage = "Ülke seçimi gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ülke adı geçerli olmalıdır")]
        [Display(Name = "Ülke")]
        public string Country { get; set; } = null!;

        [Required(ErrorMessage = "Adres gereklidir")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Adres 5-200 karakter arasında olmalıdır")]
        [Display(Name = "Adres")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakterden oluşmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Şifre en az bir küçük harf, bir büyük harf, bir sayı ve bir özel karakter içermelidir")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrarı")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; } = null!;
    }

    /// <summary>
    /// Profil bilgilerini göstermek için kullanılan DTO
    /// </summary>
    public class AuthDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = null!;

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Adresi")]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [Display(Name = "Ülke")]
        public string Country { get; set; } = null!;

        [Required]
        [Display(Name = "Adres")]
        public string Address { get; set; } = null!;

        [Display(Name = "Üyelik Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Profil güncelleme için kullanılan DTO
    /// </summary>
    public class UpdateDto
    {
        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon Numarası")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Ülke seçimi gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ülke adı geçerli olmalıdır")]
        [Display(Name = "Ülke")]
        public string Country { get; set; } = null!;

        [Required(ErrorMessage = "Adres gereklidir")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Adres 5-200 karakter arasında olmalıdır")]
        [Display(Name = "Adres")]
        public string Address { get; set; } = null!;
    }

    /// <summary>
    /// Şifre değiştirmek için kullanılan DTO (İsteğe bağlı)
    /// </summary>
    

    /// <summary>
    /// Şifre sıfırlama için kullanılan DTO
    /// </summary>
    
    /// <summary>
    /// Login yanıtı için kullanılan DTO
    /// </summary>
    public class LoginResponseDto
    {
        [Display(Name = "Başarı Durumu")]
        public bool Success { get; set; }

        [Display(Name = "Mesaj")]
        public string Message { get; set; } = null!;

        [Display(Name = "JWT Token")]
        public string? Token { get; set; }

        [Display(Name = "Refresh Token")]
        public string? RefreshToken { get; set; }

        [Display(Name = "Kullanıcı Bilgileri")]
        public AuthDto? User { get; set; }

        [Display(Name = "Hata Detayları")]
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Register yanıtı için kullanılan DTO
    /// </summary>
    public class RegisterResponseDto
    {
        [Display(Name = "Başarı Durumu")]
        public bool Success { get; set; }

        [Display(Name = "Mesaj")]
        public string Message { get; set; } = null!;

        [Display(Name = "Kullanıcı ID")]
        public int? UserId { get; set; }

        [Display(Name = "Kullanıcı Bilgileri")]
        public AuthDto? User { get; set; }

        [Display(Name = "Hata Detayları")]
        public string? ErrorDetails { get; set; }
    }

}