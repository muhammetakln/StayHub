using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
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

        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin")) return Redirect("/admin/hotel");
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

            if (user == null)
            {
                ModelState.AddModelError("", "E-posta/Kullanıcı adı veya şifre hatalı.");
                return View(dto);
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (dto.UserType == "Admin" && !roles.Contains("Admin"))
            {
                ModelState.AddModelError("", "Bu hesap Admin yetkisine sahip değil. Lütfen Müşteri girişini kullanın.");
                return View(dto);
            }

            if (dto.UserType == "Guest" && roles.Contains("Admin"))
            {
                ModelState.AddModelError("", "Admin hesapları Müşteri girişinden giriş yapamaz. Lütfen Admin girişini seçin.");
                return View(dto);
            }

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Giriş başarılı: {Email}", user.Email);

                if (roles.Contains("Admin"))
                {
                    // ✅ 404 Hatasını Önleyen Düzeltme: Doğrudan Rota Adresine Yönlendir
                    if (user.HotelId.HasValue)
                    {
                        return Redirect($"/admin/hotel?hotelId={user.HotelId.Value}");
                    }
                    return Redirect("/admin/hotel");
                }

                return RedirectToLocal(returnUrl ?? "/hotel");
            }

            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(dto);
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
                var callbackUrl = Url.Action("ResetPassword", "Account",
                    new { email = user.Email, code = code }, protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "Şifre Sıfırlama",
                    $"Şifrenizi sıfırlamak için <a href='{callbackUrl}'>buraya tıklayınız</a>.");
            }

            TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            return RedirectToAction("Login");
        }

        [HttpGet("reset-password")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? code = null, string? email = null)
        {
            if (code == null || email == null) return RedirectToAction("Login");
            return View(new ResetPasswordViewModel { Token = code, Email = email });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ResetPasswordAsync(user, model.Token!, model.NewPassword!);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
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
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Guest");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Hotel");
            }
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
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

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return Redirect("/");
        }
    }
}