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
                // 🛡️ Oturumu kontrol et
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Admin paneline erişmeye çalışan kullanıcı bulunamadı, login'e yönlendiriliyor.");
                    return RedirectToAction("Login", "Account");
                }

                bool isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");

                // Kullanıcının bağlı bir oteli varsa detayları getir
                if (user.HotelId.HasValue)
                {
                    // 🔥 Servisimiz artık TodayCheckIns, TodayCheckOuts vb. verileri de hesaplayıp getiriyor.
                    var hotelDetail = await _hotelService.GetHotelByIdAsync(user.HotelId.Value);

                    if (hotelDetail != null)
                    {
                        hotelDetail.Rooms ??= new List<RoomDto>();
                        hotelDetail.AddOnServices ??= new List<AddOnServiceDto>();
                        return View(hotelDetail);
                    }
                }

                // Oteli olmayan normal admini oluşturma sayfasına gönder
                if (!isSuperAdmin)
                {
                    _logger.LogInformation("Admin kullanıcısının oteli yok, oluşturma sayfasına yönlendiriliyor.");
                    return RedirectToAction(nameof(Create));
                }

                // SuperAdmin için genel bir boş dashboard
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
            // Zaten oteli olan normal admini dashboard'a geri yolla
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
                        // Kimlik bilgilerini (claims) hemen güncellemek için kritik
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
                CheckOutTime = hotel.CheckOutTime
            };
            return View(updateDto);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateHotelDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && (user == null || user.HotelId != id)) return Forbid();

            await _hotelService.UpdateHotelAsync(id, dto);
            TempData["SuccessMessage"] = "Otel bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
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