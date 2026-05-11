using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Route("admin/hotel")]
    public class AdminHotelController : Controller
    {
        private readonly IHotelService _hotelService;
        private readonly UserManager<Guest> _userManager;
        private readonly SignInManager<Guest> _signInManager;
        private readonly ILogger<AdminHotelController> _logger;

        public AdminHotelController(
            IHotelService hotelService,
            UserManager<Guest> userManager,
            SignInManager<Guest> signInManager,
            ILogger<AdminHotelController> logger)
        {
            _hotelService = hotelService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Admin paneline erişmeye çalışan kullanıcı bulunamadı, login'e yönlendiriliyor.");
                    return RedirectToAction("Login", "Account");
                }

                bool isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");

                if (user.HotelId.HasValue)
                {
                    var hotelDetail = await _hotelService.GetHotelByIdAsync(user.HotelId.Value);

                    if (hotelDetail != null)
                    {
                        hotelDetail.Rooms ??= new List<RoomDto>();
                        hotelDetail.AddOnServices ??= new List<AddOnServiceDto>();
                        return View(hotelDetail);
                    }
                }

                if (!isSuperAdmin)
                {
                    _logger.LogInformation("Admin kullanıcısının oteli yok, oluşturma sayfasına yönlendiriliyor.");
                    return RedirectToAction(nameof(Create));
                }

                return View(new HotelDetailDto
                {
                    Name = "Sistem Yönetim Paneli",
                    Rooms = new List<RoomDto>(),
                    AddOnServices = new List<AddOnServiceDto>(),
                    TodayCheckIns = 0,
                    TodayCheckOuts = 0,
                    ActiveReservations = 0,
                    MonthlyEarning = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yönetim paneli Index metodu sırasında hata!");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.HotelId.HasValue && !User.IsInRole("SuperAdmin"))
                return RedirectToAction(nameof(Index));

            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHotelDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return View(dto);

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                int createdHotelId = await _hotelService.CreateHotelAsync(dto);

                if (createdHotelId > 0 && !user.HotelId.HasValue)
                {
                    user.HotelId = createdHotelId;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (updateResult.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                    }
                }

                TempData["SuccessMessage"] = "Oteliniz başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Otel eklenirken bir hata oluştu.");
                return View(dto);
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && (user == null || user.HotelId != id)) return Forbid();

            var hotel = await _hotelService.GetHotelByIdAsync(id);
            if (hotel == null) return NotFound();

            var updateDto = new UpdateHotelDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address,
                PhoneNumber = hotel.PhoneNumber,
                Email = hotel.Email,
                Description = hotel.Description,
                City = hotel.City,
                Country = hotel.Country,
                StarRating = hotel.StarRating,
                IsActive = hotel.IsActive,
                CoverImageUrl = hotel.CoverImageUrl,
                CheckInTime = hotel.CheckInTime,
                CheckOutTime = hotel.CheckOutTime,

                AddOnServices = hotel.AddOnServices?.Select(s => new UpdateAddOnServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                }).ToList() ?? new List<UpdateAddOnServiceDto>()
            };
            return View(updateDto);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Edit(int id, UpdateHotelDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning("Otel güncellenirken validasyon hatası: {Errors}", errors);
                return View(dto);
            }

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && (user == null || user.HotelId != id)) return Forbid();

            try
            {
                await _hotelService.UpdateHotelAsync(id, dto);

                // ✅ DEĞİŞİKLİK BURADA: Başarılı mesajı set ediliyor ve AYNI SAYFAYA yönlendiriliyor
                TempData["SuccessMessage"] = "Otel bilgileri ve ek hizmetler başarıyla güncellendi!";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel güncellenirken hata oluştu.");
                TempData["ErrorMessage"] = "Sistemsel bir hata oluştu, lütfen tekrar deneyin.";
                return View(dto);
            }
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && (user == null || user.HotelId != id)) return Forbid();

            await _hotelService.DeleteHotelAsync(id);

            if (user != null && user.HotelId == id)
            {
                user.HotelId = null;
                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["SuccessMessage"] = "Otel silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}