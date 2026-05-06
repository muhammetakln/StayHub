using Core.Abstracts.IRepositories;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Manage.Internal;
using System;
using System.Threading.Tasks;
using Utils.Responses;

namespace Core.Abstracts.Interfaces  // ← Değişti
{
    public interface IAuthService
    {
        // Login
        Task<LoginResponseDto> LoginAsync(LoginDto dto);

        // Register
        Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);

        // Profile
        // Password
        Task<IResult> ChangePasswordAsync(int guestId, ChangePasswordDto dto);
        Task<IResult> ResetPasswordAsync(ResetPasswordDto dto);

        // Tokens
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task<IResult> LogoutAsync(int guestId);

        // Email
        Task<IResult> SendEmailVerificationAsync(string email);
        Task<IResult> VerifyEmailAsync(string email, string token);

        // Checks
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UserExistsAsync(string firstName, string lastName);
        Task<bool> IsUserActiveAsync(int guestId);  // ← Fixed typo
    }
}