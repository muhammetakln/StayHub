using System.ComponentModel.DataAnnotations;

namespace UI.Web.Models
{
    public class ProfileViewModel
    {
        // ✅ Kullanıcı Adı Alanı Eklendi
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-20 karakter arasında olmalıdır.")]
        [Display(Name = "Kullanıcı Adınız")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Adınız")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyadınız")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta Adresiniz")]
        public string Email { get; set; } = null!;

        // --- Şifre Değiştirme Alanları ---
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler birbiriyle uyuşmuyor.")]
        public string? ConfirmPassword { get; set; }
    }
}