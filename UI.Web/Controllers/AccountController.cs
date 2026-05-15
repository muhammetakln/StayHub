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
using System.Security.Claims;
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
        private readonly IAuthService _authService;

        public AccountController(
          SignInManager<Guest> signInManager,
          UserManager<Guest> userManager,
          ILogger<AccountController> logger,
          IEmailSender emailSender,
          IAuthService authService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _authService = authService;
        }

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

            // ÖNCE e-posta ile kullanıcıyı bulmaya çalışıyoruz
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Eğer e-posta ile bulamazsak, girilen değeri KULLANICI ADI (UserName) olarak arıyoruz
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(dto.Email);
            }

            // İki türlü de bulunamadıysa hata dönüyoruz
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "E-posta/Kullanıcı Adı veya şifre hatalı.");
                return View(dto);
            }

            // Kullanıcı bulundu, e-posta onay durumunu kontrol ediyoruz
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Lütfen giriş yapmadan önce e-posta adresinizi onaylayın.");
                return View(dto);
            }

            // DTO içindeki 'Email' alanını, veritabanındaki asıl e-posta ile eziyoruz. 
            // Böylece Auth Service, kullanıcı adıyla girilmiş olsa bile doğru e-posta üzerinden giriş işlemini yapabilir.
            dto.Email = user.Email!;

            var response = await _authService.LoginAsync(dto);

            if (response.Success && response.User != null)
            {
                var roles = await _userManager.GetRolesAsync(user!);
                bool isAdminUser = roles.Contains("Admin") || roles.Contains("SuperAdmin");

                _logger.LogInformation("Kullanıcı giriş yaptı: {Email}", user!.Email);

                if (isAdminUser) return Redirect("/admin/hotel");
                return RedirectToLocal(returnUrl ?? "/hotel");
            }

            ModelState.AddModelError(string.Empty, response.Message ?? "E-posta/Kullanıcı Adı veya şifre hatalı.");
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

            var response = await _authService.RegisterAsync(dto);

            if (response.Success && response.UserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(response.UserId.Value.ToString());

                // ✅ 18 Yaş kontrolü politikası için doğum tarihini Claim olarak ekliyoruz
                await _userManager.AddClaimAsync(user!, new Claim("DateOfBirth", dto.DateOfBirth.ToString("yyyy-MM-dd")));

                // ✅ E-posta onay token'ı oluşturma ve gönderme
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
                var callbackUrl = Url.Action(
                  "ConfirmEmail",
                  "Account",
                  new { userId = user!.Id, token = code },
                  protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(dto.Email, "StayHub - Hesabınızı Onaylayın",
                  $"Lütfen hesabınızı onaylamak için <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>buraya tıklayın</a>.");

                TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapabilmek için e-posta adresinize gönderilen onay bağlantısına tıklayın.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, response.Message);
            if (!string.IsNullOrEmpty(response.ErrorDetails))
            {
                _logger.LogWarning("Kayıt Hatası: {Details}", response.ErrorDetails);
            }

            return View(dto);
        }

        [HttpGet("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound($"ID'si '{userId}' olan kullanıcı bulunamadı.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "E-posta adresiniz başarıyla onaylandı. Artık giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }

            return BadRequest("E-posta onaylanırken bir hata oluştu.");
        }

        [HttpGet("forgot-password")]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction(nameof(ForgotPasswordConfirmation));

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(model.Email, "StayHub - Şifre Sıfırlama Talebi",
              $"Hesabınızın şifresini sıfırlamak için lütfen <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>buraya tıklayın</a>.");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet("forgot-password-confirmation")]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string code)
        {
            if (userId == null || code == null) return BadRequest("Geçersiz veya eksik şifre sıfırlama bağlantısı.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("Kullanıcı bulunamadı.");

            return View(new ResetPasswordViewModel { Token = code, Email = user.Email! });
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("Index", "Home");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellenmiştir.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return Redirect("/");
        }
    }
}