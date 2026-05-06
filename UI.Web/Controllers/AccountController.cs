using Business.Services;
using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using UI.Web.Models;
using UI.Web.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace UI.Web.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<Guest> _signInManager;
        private readonly UserManager<Guest> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(
            IAuthService authService,
            SignInManager<Guest> signInManager,
            UserManager<Guest> userManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender)
        {
            _authService = authService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        // GET: /account/login
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /account/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string returnUrl = null)
        {
            try
            {
                _logger.LogInformation("Login deneme: {Email}", dto.Email);

                if (!ModelState.IsValid) return View(dto);

                // ✅ Önce E-posta ile ara, bulamazsan Kullanıcı Adı ile ara
                var user = await _userManager.FindByEmailAsync(dto.Email)
                           ?? await _userManager.FindByNameAsync(dto.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                    return View(dto);
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                bool isAdmin = userRoles.Contains("Admin");
                bool isGuest = userRoles.Contains("Guest");

                if (dto.UserType == "Admin" && !isAdmin)
                {
                    ModelState.AddModelError("", "Bu hesap Admin yetkisine sahip değil.");
                    return View(dto);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, // Giriş işlemini UserName üzerinden yapıyoruz
                    dto.Password,
                    dto.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (isAdmin && dto.UserType == "Admin") return RedirectToLocal(returnUrl ?? "/admin/hotel");
                    return RedirectToLocal(returnUrl ?? "/hotel");
                }

                ModelState.AddModelError("", "Şifre hatalı.");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login hatası");
                return View(dto);
            }
        }

        // ✅ GET: /account/register (EKLENDİ - 405 Hatasını Çözer)
        [HttpGet("register")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /account/register
        [HttpPost("register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = new Guest
            {
                UserName = dto.Email, // İlk kayıtta UserName = Email (Sonradan değiştirilebilir)
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Guest");
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "Kayıt başarılı!";
                return RedirectToAction("Index", "Hotel");
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(dto);
        }

        // GET: /account/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName // ✅ Kullanıcı adı modele eklendi
            };
            return View(model);
        }

        // POST: /account/profile
        [HttpPost("profile")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            // ✅ E-posta Değişikliği
            if (user.Email != model.Email)
            {
                await _userManager.SetEmailAsync(user, model.Email);
            }

            // ✅ Kullanıcı Adı Değişikliği (E-postadan bağımsız)
            if (user.UserName != model.UserName)
            {
                var userNameExists = await _userManager.FindByNameAsync(model.UserName);
                if (userNameExists != null)
                {
                    ModelState.AddModelError("UserName", "Bu kullanıcı adı zaten alınmış.");
                    return View(model);
                }
                await _userManager.SetUserNameAsync(user, model.UserName);
            }

            var updateResult = await _userManager.UpdateAsync(user);

            // Şifre Değiştirme Bölümü
            if (updateResult.Succeeded && !string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var passResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passResult.Succeeded)
                {
                    foreach (var error in passResult.Errors) ModelState.AddModelError("", error.Description);
                    return View(model);
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Profiliniz güncellendi.";
            return RedirectToAction("Profile");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
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
            if (user != null)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { email = user.Email, code = code }, protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Şifre Sıfırlama", $"Şifrenizi sıfırlamak için <a href='{callbackUrl}'>tıklayınız</a>.");
            }
            TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            return RedirectToAction("Login");
        }

        [HttpGet("reset-password")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null, string email = null) => View(new ResetPasswordViewModel { Token = code, Email = email });

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("Login");
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı.";
                return RedirectToAction("Login");
            }
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl) => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("/");
    }
}