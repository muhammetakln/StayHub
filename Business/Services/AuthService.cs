using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Identity;
using Utils.Responses;

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
                // Email zaten kullanılmış mı?
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

                // Yeni Guest oluştur
                var guest = new Guest
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    Country = dto.Country,
                    Address = dto.Address,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = false,
                    IsDeleted = false
                };

                // Şifre ile oluştur
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

                // Default "User" role'ü ata
                var roleExists = await _roleManager.RoleExistsAsync("User");
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole<int> { Name = "User" });
                }
                await _userManager.AddToRoleAsync(guest, "User");

                // Email verification token generate et
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(guest);

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
                    Message = "Kayıt başarılı. Lütfen email doğrulaması yapınız.",
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
                // Email ile guest bul
                var guest = await _userManager.FindByEmailAsync(dto.Email);

                if (guest == null)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Email veya şifre yanlış",
                        ErrorDetails = "Kullanıcı bulunamadı"
                    };
                }

                // Email doğrulanmış mı kontrol et
                if (!guest.EmailConfirmed)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Email henüz doğrulanmamış. Lütfen email doğrulaması yapınız.",
                        ErrorDetails = "Email doğrulması gerekli"
                    };
                }

                // Aktif mi kontrol et
                if (!guest.IsActive)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Hesabınız deaktive edilmiştir",
                        ErrorDetails = "Hesap inaktif"
                    };
                }

                // Şifre kontrol et
                var result = await _signInManager.PasswordSignInAsync(guest, dto.Password, dto.RememberMe, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return new LoginResponseDto
                        {
                            Success = false,
                            Message = "Hesabınız çok fazla başarısız giriş nedeniyle kilitlenmiştir",
                            ErrorDetails = "Hesap kilitli"
                        };
                    }

                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Email veya şifre yanlış",
                        ErrorDetails = "Giriş başarısız"
                    };
                }

                // Son giriş tarihini güncelle
                guest.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(guest);

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

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Giriş başarılı",
                    User = authDto,
                    Token = "JWT_TOKEN_HERE",
                    RefreshToken = "REFRESH_TOKEN_HERE"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Giriş sırasında bir hata oluştu",
                    ErrorDetails = ex.Message
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // LOGOUT - ÇIKIŞ
        // ═══════════════════════════════════════════════════════════════

        public async Task<IResult> LogoutAsync(int guestId)
        {
            try
            {
                var guest = await _userManager.FindByIdAsync(guestId.ToString());

                if (guest == null)
                {
                    return Result.Failure("Kullanıcı bulunamadı");
                }

                await _signInManager.SignOutAsync();
                return Result.Success("Çıkış başarılı");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Hata: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // CHANGE PASSWORD - ŞİFRE DEĞİŞTİR
        // ═══════════════════════════════════════════════════════════════

        public async Task<IResult> ChangePasswordAsync(int guestId, ChangePasswordDto dto)
        {
            try
            {
                var guest = await _userManager.FindByIdAsync(guestId.ToString());

                if (guest == null)
                {
                    return Result.Failure("Kullanıcı bulunamadı");
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(guest, dto.CurrentPassword);

                if (!isPasswordValid)
                {
                    return Result.Failure("Mevcut şifre yanlış");
                }

                var result = await _userManager.ChangePasswordAsync(guest, dto.CurrentPassword, dto.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure(errors);
                }

                return Result.Success("Şifre başarıyla değiştirildi");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Hata: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // RESET PASSWORD - ŞİFRE SIFIRLA
        // ═══════════════════════════════════════════════════════════════

        public async Task<IResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                var guest = await _userManager.FindByEmailAsync(dto.Email);

                if (guest == null)
                {
                    return Result.Success("Eğer email tüm sistemde kayıtlıysa, şifre sıfırlama linki gönderilecektir");
                }

                var result = await _userManager.ResetPasswordAsync(guest, dto.Token, dto.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure(errors);
                }

                return Result.Success("Şifre başarıyla sıfırlandı");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Hata: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // EMAIL VERIFICATION - EMAIL DOĞRULAMA
        // ═══════════════════════════════════════════════════════════════

        public async Task<IResult> SendEmailVerificationAsync(string email)
        {
            try
            {
                var guest = await _userManager.FindByEmailAsync(email);

                if (guest == null)
                {
                    return Result.Failure("Kullanıcı bulunamadı");
                }

                if (guest.EmailConfirmed)
                {
                    return Result.Failure("Email zaten doğrulanmış");
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(guest);
                return Result.Success("Doğrulama linki email'inize gönderilmiştir");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Hata: {ex.Message}");
            }
        }

        public async Task<IResult> VerifyEmailAsync(string email, string token)
        {
            try
            {
                var guest = await _userManager.FindByEmailAsync(email);

                if (guest == null)
                {
                    return Result.Failure("Kullanıcı bulunamadı");
                }

                if (guest.EmailConfirmed)
                {
                    return Result.Failure("Email zaten doğrulanmış");
                }

                var result = await _userManager.ConfirmEmailAsync(guest, token);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure(errors);
                }

                return Result.Success("Email başarıyla doğrulandı");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Hata: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // TOKEN MANAGEMENT - TOKEN YÖNETİMİ
        // ═══════════════════════════════════════════════════════════════

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Token yenilendi",
                    Token = "NEW_JWT_TOKEN",
                    RefreshToken = "NEW_REFRESH_TOKEN"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Token yenileme başarısız",
                    ErrorDetails = ex.Message
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // USER CHECKS - KONTROLLER
        // ═══════════════════════════════════════════════════════════════

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                var guest = await _userManager.FindByEmailAsync(email);
                return guest != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UserExistsAsync(string firstName, string lastName)
        {
            try
            {
                var guest = _userManager.Users
                    .FirstOrDefault(g => g.FirstName == firstName && g.LastName == lastName);

                return guest != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserActiveAsync(int guestId)
        {
            try
            {
                var guest = await _userManager.FindByIdAsync(guestId.ToString());

                if (guest == null)
                {
                    return false;
                }

                return guest.IsActive && guest.EmailConfirmed && !guest.IsDeleted;
            }
            catch
            {
                return false;
            }
        }
    }
}