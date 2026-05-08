using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using UI.Web.Models;
using UI.Web.ViewModels;

namespace UI.Web.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly SignInManager<Guest> _signInManager;
        private readonly UserManager<Guest> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(
            SignInManager<Guest> signInManager,
            UserManager<Guest> userManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        // --- PROFİL VE ŞİFRE GÜNCELLEME ---

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new ProfileViewModel
            {
                UserName = user.UserName ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? ""
            };

            return View(model);
        }

        [HttpPost("profile")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            user.UserName = model.UserName;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors) ModelState.AddModelError("", error.Description);
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Şifre değiştirmek için mevcut şifrenizi girmelisiniz.");
                    return View(model);
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors) ModelState.AddModelError("", error.Description);
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = "Profil ve güvenlik bilgileriniz başarıyla güncellendi.";
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction(nameof(Profile));
        }

        // --- GİRİŞ / ÇIKIŞ İŞLEMLERİ ---

        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return Redirect("/admin/hotel");
                return RedirectToAction("Index", "Hotel");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _userManager.FindByEmailAsync(dto.Email)
                       ?? await _userManager.FindByNameAsync(dto.Email);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                return View(dto);
            }

            var roles = await _userManager.GetRolesAsync(user);
            bool isAdminUser = roles.Contains("Admin") || roles.Contains("SuperAdmin");

            if (dto.UserType == "Admin" && !isAdminUser)
            {
                ModelState.AddModelError(string.Empty, "Yetkisiz erişim denemesi.");
                return View(dto);
            }

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Kullanıcı giriş yaptı: {Email}", user.Email);
                if (isAdminUser) return Redirect("/admin/hotel");
                return RedirectToLocal(returnUrl ?? "/hotel");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Çok fazla hatalı deneme. Hesabınız geçici olarak kilitlendi.");
                return View(dto);
            }

            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(dto);
        }

        [HttpPost("logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpGet("register")]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost("register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = new Guest
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,

                IdentificationNumber = "00000000000",
                Address = "Belirtilmedi",
                Country = "Türkiye",
                DateOfBirth = DateTime.Now.AddYears(-18)
            };

            try
            {
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Guest");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Hotel");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt esnasında veritabanı hatası oluştu.");
                ModelState.AddModelError(string.Empty, "Sistemsel bir hata oluştu. Lütfen bilgilerinizi kontrol edin.");
            }

            return View(dto);
        }

        // --- ŞİFREMİ UNUTTUM İŞLEMLERİ ---

        [HttpGet("forgot-password")]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                model.Email,
                "StayHub - Şifre Sıfırlama Talebi",
                $"Hesabınızın şifresini sıfırlamak için lütfen <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>buraya tıklayın</a>.");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet("forgot-password-confirmation")]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // --- ŞİFRE YENİLEME (RESET PASSWORD) İŞLEMLERİ ---

        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("Geçersiz veya eksik şifre sıfırlama bağlantısı.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Kullanıcı bulunamadı.");
            }

            var model = new ResetPasswordViewModel
            {
                Token = code,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Güvenlik gereği kullanıcı yoksa bile çaktırmadan Ana Sayfaya yönlendiriyoruz
                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellenmiştir.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (result.Succeeded)
            {
                // İşlem başarılıysa Ana Sayfaya (Home/Index) yönlendiriyoruz
                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellenmiştir. Yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // --- YARDIMCI METOTLAR ---

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return Redirect("/");
        }
    }
}