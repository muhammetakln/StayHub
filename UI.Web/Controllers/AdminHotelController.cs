using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/hotel")]
    public class AdminHotelController : Controller
    {
        private readonly IHotelService _hotelService;
        private readonly UserManager<Guest> _userManager;
        private readonly ILogger<AdminHotelController> _logger;

        public AdminHotelController(
            IHotelService hotelService,
            UserManager<Guest> userManager,
            ILogger<AdminHotelController> logger)
        {
            _hotelService = hotelService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null && user.HotelId.HasValue)
                {
                    var hotelDetail = await _hotelService.GetHotelByIdAsync(user.HotelId.Value);

                    if (hotelDetail != null)
                    {
                        // ✅ HotelDetailDto'yu HotelDto'ya dönüştürüyoruz
                        var hotelDto = new HotelDto
                        {
                            Id = hotelDetail.Id,
                            Name = hotelDetail.Name,
                            City = hotelDetail.City,
                            Country = hotelDetail.Country,
                            IsActive = hotelDetail.IsActive,
                            StarRating = (int)hotelDetail.Rating
                        };

                        return View(new List<HotelDto> { hotelDto });
                    }
                }

                var allHotels = await _hotelService.GetHotelsAsync();
                return View(allHotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yönetim paneli yüklenirken hata oluştu");
                return View(new List<HotelDto>());
            }
        }

        // ✅ GET: /admin/hotel/create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            // 🛡️ Sadece Süper Adminler (OtelId'si olmayanlar) yeni otel yaratabilir
            if (user != null && user.HotelId.HasValue)
            {
                TempData["ErrorMessage"] = "Yeni otel oluşturma yetkiniz bulunmamaktadır.";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // ✅ POST: /admin/hotel/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHotelDto dto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.HotelId.HasValue) return Forbid();

                if (!ModelState.IsValid) return View(dto);

                await _hotelService.CreateHotelAsync(dto);
                TempData["SuccessMessage"] = "Otel başarıyla sisteme eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel oluşturma hatası");
                ModelState.AddModelError("", "Otel eklenirken bir hata oluştu.");
                return View(dto);
            }
        }

        // ✅ GET: /admin/hotel/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // 🛡️ GÜVENLİK: Admin sadece kendi otelinin detayına bakabilir
                if (user != null && user.HotelId.HasValue && user.HotelId.Value != id)
                {
                    return Forbid();
                }

                var hotel = await _hotelService.GetHotelByIdAsync(id);
                if (hotel == null) return NotFound();

                return View(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel detayı yüklenirken hata");
                return RedirectToAction(nameof(Index));
            }
        }

        // ✅ GET: /admin/hotel/delete/{id}
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // 🛡️ GÜVENLİK: Admin sadece kendi otelini silebilir
                if (user != null && user.HotelId.HasValue && user.HotelId.Value != id)
                {
                    return Forbid();
                }

                var hotel = await _hotelService.GetHotelByIdAsync(id);
                if (hotel == null) return NotFound();

                return View(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel silme sayfası yüklenirken hata");
                return RedirectToAction(nameof(Index));
            }
        }

        // ✅ POST: /admin/hotel/delete/{id}
        [HttpPost("delete/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null && user.HotelId.HasValue && user.HotelId.Value != id)
                {
                    return Forbid();
                }

                await _hotelService.DeleteHotelAsync(id);
                TempData["SuccessMessage"] = "Otel sistemden başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel silme hatası");
                TempData["ErrorMessage"] = "Otel silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}