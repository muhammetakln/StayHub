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

namespace UI.Web.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<Guest> _signInManager;
        private readonly UserManager<Guest> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthService authService,
            SignInManager<Guest> signInManager,
            UserManager<Guest> userManager,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
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
                _logger.LogInformation("Login deneme: {Email}, UserType: {UserType}", dto.Email, dto.UserType);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Login validation başarısız");
                    return View(dto);
                }

                // Kullanıcıyı bul
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email veya şifre yanlışdır.");
                    _logger.LogWarning("Kullanıcı bulunamadı: {Email}", dto.Email);
                    return View(dto);
                }

                // Kullanıcı rollerini kontrol et
                var userRoles = await _userManager.GetRolesAsync(user);
                bool isAdmin = userRoles.Contains("Admin");
                bool isGuest = userRoles.Contains("Guest");

                // Admin olarak giriş yapmaya çalışıyorsa ama Admin değilse
                if (dto.UserType == "Admin" && !isAdmin)
                {
                    ModelState.AddModelError("", "Bu hesap Admin olarak kaydedilmemiştir.");
                    _logger.LogWarning("Admin giriş başarısız (yetki yok): {Email}", dto.Email);
                    return View(dto);
                }

                // Guest olarak giriş yapmaya çalışıyorsa ama Guest değilse
                if (dto.UserType == "Guest" && !isGuest)
                {
                    ModelState.AddModelError("", "Bu hesap Guest olarak kaydedilmemiştir.");
                    _logger.LogWarning("Guest giriş başarısız (yetki yok): {Email}", dto.Email);
                    return View(dto);
                }

                // SignIn yap
                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    dto.Password,
                    dto.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Başarılı login: {Email}, Role: {UserType}", dto.Email, dto.UserType);

                    // Admin ise /admin/hotel'e yönlendir
                    if (isAdmin && dto.UserType == "Admin")
                    {
                        return RedirectToLocal(returnUrl ?? "/admin/hotel");
                    }
                    // Guest ise /hotel'e yönlendir
                    else if (isGuest && dto.UserType == "Guest")
                    {
                        return RedirectToLocal(returnUrl ?? "/hotel");
                    }
                    else
                    {
                        return RedirectToLocal(returnUrl ?? "/");
                    }
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("Hesap kilitli: {Email}", dto.Email);
                    ModelState.AddModelError("", "Hesabınız kilitlidir. Daha sonra tekrar deneyin.");
                }
                else
                {
                    _logger.LogWarning("Başarısız login: {Email}", dto.Email);
                    ModelState.AddModelError("", "Email veya şifre yanlışdır.");
                }

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login hatası");
                ModelState.AddModelError("", "Giriş yapılırken hata oluştu");
                return View(dto);
            }
        }

        // GET: /account/register
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
            try
            {
                _logger.LogInformation("Kayıt başlatıldı: {Email}", dto.Email);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Register validation başarısız");
                    return View(dto);
                }

                var user = new Guest
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber ?? "",
                    Country = dto.Country ?? "Turkey",
                    Address = dto.Address ?? "",
                    IdentificationNumber = dto.IdentificationNumber ?? "",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (result.Succeeded)
                {
                    // Guest role'üne ekle
                    await _userManager.AddToRoleAsync(user, "Guest");

                    _logger.LogInformation("Yeni kullanıcı kaydı: {Email}", dto.Email);

                    // Otomatik login yap
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["SuccessMessage"] = "Hoşgeldiniz! Kayıt başarılı.";
                    // Guest kaydedildiyse /hotel'e yönlendir
                    return RedirectToAction("Index", "Hotel");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    _logger.LogWarning("Register hatası: {Email}", dto.Email);
                }

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register hatası");
                ModelState.AddModelError("", "Kayıt yapılırken hata oluştu");
                return View(dto);
            }
        }

        // POST: /account/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation("Logout: {Email}", user?.Email);

                await _signInManager.SignOutAsync();
                TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout hatası");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /account/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                var dto = new GuestProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Country = user.Country,
                    Address = user.Address,
                };

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile hatası");
                return RedirectToAction("Index", "Home");
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }
    }
}