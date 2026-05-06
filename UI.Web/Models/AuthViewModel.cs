using System.ComponentModel.DataAnnotations;

namespace UI.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = null!;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Şifre en az 1 büyük harf, 1 küçük harf ve 1 rakam içermelidir")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        [Display(Name = "Şifre Tekrarı")]
        public string ConfirmPassword { get; set; } = null!;

        [Display(Name = "Telefon (İsteğe Bağlı)")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Şartları Kabul Ediyorum")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Şartları kabul etmelisiniz")]
        public bool AcceptTerms { get; set; }
    }

    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "Yeni email gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Yeni Email")]
        public string NewEmail { get; set; } = null!;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifreniz")]
        public string Password { get; set; } = null!;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Yeni şifre gereklidir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Şifre en az 1 büyük harf, 1 küçük harf ve 1 rakam içermelidir")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        public string ConfirmPassword { get; set; } = null!;
    }

    // ✅ YENİ EKLENENLER:
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email Adresiniz")]
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Şifre en az 1 büyük harf, 1 küçük harf ve 1 rakam içermelidir")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor.")]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        public string ConfirmPassword { get; set; } = null!;

        public string Token { get; set; } = null!; // URL'den gelecek güvenlik anahtarı
    }
}