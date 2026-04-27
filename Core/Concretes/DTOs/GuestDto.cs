using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs { 
public class GuestDto
{
    [Display(Name = "ID")]
    public int Id { get; set; }

    [Display(Name = "Kullanıcı Adı")]
    public string? UserName { get; set; }

    [Display(Name = "Adı")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Soyadı")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Tam Ad")]
    public string FullName => $"{FirstName} {LastName}";

    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Display(Name = "Telefon")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Profil Resmi")]
    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [Display(Name = "İlçe")]
    public string? District { get; set; }

    [Display(Name = "Ülke")]
    public string? Country { get; set; }

    [Display(Name = "Email Doğrulandı mı")]
    public bool IsEmailVerified { get; set; }

    [Display(Name = "Aktif mi")]
    public bool IsActive { get; set; }

    [Display(Name = "Hesap Kilitli mi")]
    public bool IsLocked { get; set; }

    [Display(Name = "Kayıt Tarihi")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Son Giriş Tarihi")]
    public DateTime? LastLoginDate { get; set; }

    [Display(Name = "Roller")]
    public ICollection<string>? Roles { get; set; } = new List<string>();
}

/// <summary>
/// Guest List DTO'su (Paginasyon için)
/// </summary>
public class GuestListDto
{
    [Display(Name = "ID")]
    public int Id { get; set; }

    [Display(Name = "Tam Ad")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Display(Name = "Telefon")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [Display(Name = "Aktif mi")]
    public bool IsActive { get; set; }

    [Display(Name = "Kayıt Tarihi")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Roller")]
    public ICollection<string>? Roles { get; set; }
}

/// <summary>
/// Guest Oluştur DTO'su (Admin tarafından)
/// </summary>
public class CreateGuestDto
{
    [Required(ErrorMessage = "Adı boş bırakamazsınız")]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Adı")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Soyadı boş bırakamazsınız")]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Soyadı")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email adresi boş bırakamazsınız")]
    [EmailAddress]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Telefon numarası boş bırakamazsınız")]
    [Phone]
    [Display(Name = "Telefon Numarası")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Şifre boş bırakamazsınız")]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [StringLength(200)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [StringLength(50)]
    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [StringLength(50)]
    [Display(Name = "İlçe")]
    public string? District { get; set; }

    [StringLength(50)]
    [Display(Name = "Ülke")]
    public string? Country { get; set; }

    [Display(Name = "Aktif mi")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Email Doğrulandı mı")]
    public bool IsEmailVerified { get; set; } = false;

    [Display(Name = "Atanacak Roller")]
    public List<int>? RoleIds { get; set; } = new List<int>();
}

/// <summary>
/// Guest Güncelle DTO'su
/// </summary>
public class UpdateGuestDto
{
    [Required(ErrorMessage = "Adı boş bırakamazsınız")]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Adı")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Soyadı boş bırakamazsınız")]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Soyadı")]
    public string LastName { get; set; } = null!;

    [Phone]
    [Display(Name = "Telefon Numarası")]
    public string? PhoneNumber { get; set; }

    [StringLength(200)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [StringLength(50)]
    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [StringLength(50)]
    [Display(Name = "İlçe")]
    public string? District { get; set; }

    [StringLength(50)]
    [Display(Name = "Ülke")]
    public string? Country { get; set; }

    [Url]
    [Display(Name = "Profil Resmi URL'si")]
    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Aktif mi")]
    public bool IsActive { get; set; }
}

/// <summary>
/// Guest Profil DTO'su (Kendi profilini görmek için)
/// </summary>
public class GuestProfileDto
{
    [Display(Name = "ID")]
    public int Id { get; set; }

    [Display(Name = "Kullanıcı Adı")]
    public string? UserName { get; set; }

    [Display(Name = "Adı")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Soyadı")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Display(Name = "Telefon")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Profil Resmi")]
    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [Display(Name = "İlçe")]
    public string? District { get; set; }

    [Display(Name = "Ülke")]
    public string? Country { get; set; }

    [Display(Name = "Email Doğrulandı mı")]
    public bool IsEmailVerified { get; set; }

    [Display(Name = "Kayıt Tarihi")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Son Giriş Tarihi")]
    public DateTime? LastLoginDate { get; set; }

    [Display(Name = "Rezervasyon Sayısı")]
    public int ReservationCount { get; set; }

    [Display(Name = "Yorum Sayısı")]
    public int ReviewCount { get; set; }
}

// ═══════════════════════════════════════════════════════════
// GUEST - PASSWORD MANAGEMENT DTOs
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Şifreyi Değiştir DTO'su
/// </summary>
public class ChangePasswordDto
{
    [Required(ErrorMessage = "Mevcut şifreyi giriniz")]
    [DataType(DataType.Password)]
    [Display(Name = "Mevcut Şifre")]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifreyi giriniz")]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifreyi doğrulayınız")]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre Doğrulama")]
    public string ConfirmPassword { get; set; } = null!;
}

/// <summary>
/// Şifremi Unuttum DTO'su
/// </summary>
public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email adresi boş bırakamazsınız")]
    [EmailAddress]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = null!;
}

/// <summary>
/// Şifreyi Sıfırla DTO'su
/// </summary>
public class ResetPasswordDto
{
    [Required(ErrorMessage = "Email adresi boş bırakamazsınız")]
    [EmailAddress]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Token boş bırakamazsınız")]
    [Display(Name = "Reset Token")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifreyi giriniz")]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifreyi doğrulayınız")]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre Doğrulama")]
    public string ConfirmPassword { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════
// GUEST - EMAIL VERIFICATION DTOs
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Email Doğrula DTO'su
/// </summary>
public class VerifyEmailDto
{
    [Required(ErrorMessage = "Email adresi boş bırakamazsınız")]
    [EmailAddress]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Doğrulama kodu boş bırakamazsınız")]
    [Display(Name = "Doğrulama Kodu")]
    public string VerificationCode { get; set; } = null!;
}

/// <summary>
/// Doğrulama Kodu Gönder DTO'su
/// </summary>
public class ResendVerificationCodeDto
{
    [Required(ErrorMessage = "Email adresi boş bırakamazsınız")]
    [EmailAddress]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════
// GUEST - ADMIN MANAGEMENT DTOs
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Guest'e Role Atama DTO'su (Admin tarafından)
/// </summary>
public class AssignRoleDto
{
    [Required(ErrorMessage = "Guest ID boş bırakamazsınız")]
    [Display(Name = "Guest ID")]
    public int GuestId { get; set; }

    [Required(ErrorMessage = "Role ID boş bırakamazsınız")]
    [Display(Name = "Role ID")]
    public int RoleId { get; set; }
}

/// <summary>
/// Guest Lockout DTO'su (Admin tarafından)
/// </summary>
public class LockGuestDto
{
    [Required(ErrorMessage = "Guest ID boş bırakamazsınız")]
    [Display(Name = "Guest ID")]
    public int GuestId { get; set; }

    [StringLength(500)]
    [Display(Name = "Neden Kilitlendi")]
    public string? Reason { get; set; }
}

/// <summary>
/// Guest Unlock DTO'su (Admin tarafından)
/// </summary>
public class UnlockGuestDto
{
    [Required(ErrorMessage = "Guest ID boş bırakamazsınız")]
    [Display(Name = "Guest ID")]
    public int GuestId { get; set; }
}

/// <summary>
/// Toplu Guest İşlem DTO'su (Admin tarafından)
/// </summary>
public class BulkGuestActionDto
{
    [Required(ErrorMessage = "Guest ID'leri boş bırakamazsınız")]
    [Display(Name = "Guest ID'leri")]
    public List<int> GuestIds { get; set; } = new List<int>();

    [Required(ErrorMessage = "İşlem türü boş bırakamazsınız")]
    [StringLength(50)]
    [Display(Name = "İşlem Türü")]
    public string Action { get; set; } = null!; // "Activate", "Deactivate", "Delete"

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}
    }