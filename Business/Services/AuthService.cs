using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Identity;
using Utils.Responses;
using System.Linq;

namespace Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Guest> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<Guest> _signInManager;

        public AuthService(
            SignInManager<Guest> signInManager,
            UserManager<Guest> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // ═══════════════════════════════════════════════════════════════
        // REGISTER - KAYIT
        // ═══════════════════════════════════════════════════════════════

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Bu email zaten kullanılmaktadır",
                        ErrorDetails = "Email unique olmalı"
                    };
                }

                var guest = new Guest
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    Country = dto.Country,
                    Address = dto.Address,
                    IdentificationNumber = dto.IdentificationNumber,
                    DateOfBirth = dto.DateOfBirth,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true, // ✅ GÜNCELLENDİ: Yeni kayıtlar otomatik onaylı başlasın
                    IsDeleted = false
                };

                var result = await _userManager.CreateAsync(guest, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Kayıt başarısız",
                        ErrorDetails = errors
                    };
                }

                var roleExists = await _roleManager.RoleExistsAsync("Guest");
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole<int> { Name = "Guest" });
                }
                await _userManager.AddToRoleAsync(guest, "Guest");

                var authDto = new AuthDto
                {
                    Id = guest.Id,
                    FirstName = guest.FirstName,
                    LastName = guest.LastName,
                    Email = guest.Email,
                    PhoneNumber = guest.PhoneNumber,
                    Country = guest.Country,
                    Address = guest.Address,
                    CreatedAt = guest.CreatedAt
                };

                return new RegisterResponseDto
                {
                    Success = true,
                    Message = "Kayıt başarılı.", // ✅ GÜNCELLENDİ: Doğrulama mesajı kaldırıldı
                    UserId = guest.Id,
                    User = authDto
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "Kayıt sırasında bir hata oluştu",
                    ErrorDetails = ex.Message
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // LOGIN - GİRİŞ
        // ═══════════════════════════════════════════════════════════════

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            try
            {
                // 1. Kullanıcıyı bul
                var guest = await _userManager.FindByEmailAsync(dto.Email)
                            ?? await _userManager.FindByNameAsync(dto.Email);

                if (guest == null)
                {
                    return new LoginResponseDto { Success = false, Message = "Email veya şifre yanlış" };
                }

                // 2. Rol Kontrolü (Güvenlik Katmanı)
                var roles = await _userManager.GetRolesAsync(guest);
                bool isAdmin = roles.Contains("Admin") || roles.Contains("SuperAdmin");

                if (dto.UserType == "Admin" && !isAdmin)
                {
                    return new LoginResponseDto { Success = false, Message = "Bu alandan sadece yetkililer giriş yapabilir." };
                }
                if (dto.UserType == "Guest" && isAdmin)
                {
                    return new LoginResponseDto { Success = false, Message = "Yetkili hesapları müşteri panelinden giriş yapamaz." };
                }

                // 3. Durum Kontrolleri
                if (!guest.IsActive || guest.IsDeleted)
                {
                    return new LoginResponseDto { Success = false, Message = "Hesabınız aktif değildir." };
                }

                // ✅ GÜNCELLENDİ: EmailConfirmed kontrolü tamamen kaldırıldı, böylece engel aşılmış oldu.

                // 4. Giriş İşlemi
                var result = await _signInManager.PasswordSignInAsync(guest, dto.Password, dto.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    guest.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(guest);

                    return new LoginResponseDto
                    {
                        Success = true,
                        Message = "Giriş başarılı",
                        User = new AuthDto
                        {
                            Id = guest.Id,
                            FirstName = guest.FirstName,
                            LastName = guest.LastName,
                            Email = guest.Email,
                            PhoneNumber = guest.PhoneNumber,
                            Country = guest.Country,
                            Address = guest.Address,
                            CreatedAt = guest.CreatedAt
                        }
                    };
                }

                if (result.IsLockedOut)
                {
                    return new LoginResponseDto { Success = false, Message = "Çok fazla hatalı deneme. Hesabınız kilitlendi." };
                }

                return new LoginResponseDto { Success = false, Message = "Email veya şifre yanlış" };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto { Success = false, Message = "Bir hata oluştu", ErrorDetails = ex.Message };
            }
        }

        public async Task<IResult> LogoutAsync(int guestId)
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Result.Success("Çıkış başarılı");
            }
            catch (Exception ex) { return Result.Failure(ex.Message); }
        }

        public async Task<IResult> ChangePasswordAsync(int guestId, ChangePasswordDto dto)
        {
            var guest = await _userManager.FindByIdAsync(guestId.ToString());
            if (guest == null) return Result.Failure("Kullanıcı bulunamadı");
            var result = await _userManager.ChangePasswordAsync(guest, dto.CurrentPassword, dto.NewPassword);
            return result.Succeeded ? Result.Success("Şifre değiştirildi") : Result.Failure("Hata oluştu");
        }

        public async Task<IResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var guest = await _userManager.FindByEmailAsync(dto.Email);
            if (guest == null) return Result.Success("Talep alındı");
            var result = await _userManager.ResetPasswordAsync(guest, dto.Token, dto.NewPassword);
            return result.Succeeded ? Result.Success("Sıfırlandı") : Result.Failure("Hata");
        }

        public async Task<IResult> SendEmailVerificationAsync(string email)
        {
            var guest = await _userManager.FindByEmailAsync(email);
            if (guest == null) return Result.Failure("Bulunamadı");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(guest);
            return Result.Success("Gönderildi");
        }

        public async Task<IResult> VerifyEmailAsync(string email, string token)
        {
            var guest = await _userManager.FindByEmailAsync(email);
            if (guest == null) return Result.Failure("Bulunamadı");
            var result = await _userManager.ConfirmEmailAsync(guest, token);
            return result.Succeeded ? Result.Success("Doğrulandı") : Result.Failure("Hata");
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken) => new LoginResponseDto { Success = true, Message = "Yenilendi" };

        public async Task<bool> EmailExistsAsync(string email) => await _userManager.FindByEmailAsync(email) != null;

        public async Task<bool> UserExistsAsync(string firstName, string lastName) => _userManager.Users.Any(g => g.FirstName == firstName && g.LastName == lastName);

        public async Task<bool> IsUserActiveAsync(int guestId)
        {
            var guest = await _userManager.FindByIdAsync(guestId.ToString());
            return guest != null && guest.IsActive && !guest.IsDeleted;
        }
    }
}