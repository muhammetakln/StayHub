using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UI.Web.Controllers
{
    [Route("hotel")]
    public class HotelController : Controller
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelController> _logger;

        public HotelController(IHotelService hotelService, ILogger<HotelController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        // ✅ GET: /hotel
        [HttpGet]
        public async Task<IActionResult> Index(string? city)
        {
            // 1. Şehir parametresi varsa doğrudan servis üzerinden filtreli çekiyoruz (Daha performanslı)
            List<HotelDto> hotels;

            if (!string.IsNullOrWhiteSpace(city))
            {
                _logger.LogInformation($"{city} şehri için oteller filtreleniyor.");
                hotels = await _hotelService.GetHotelsByCityAsync(city);
                ViewBag.City = city;
                ViewBag.ResultCount = hotels.Count;
            }
            else
            {
                _logger.LogInformation("Tüm oteller listeleniyor.");
                hotels = await _hotelService.GetHotelsAsync();
                ViewBag.City = null;
            }

            return View(hotels);
        }

        // ✅ GET: /hotel/details/{id}
        [HttpGet("details/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0) return RedirectToAction(nameof(Index));

                var hotelDetail = await _hotelService.GetHotelByIdAsync(id);

                if (hotelDetail == null)
                {
                    TempData["ErrorMessage"] = "Otel bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                return View(hotelDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detay sayfası yüklenirken hata.");
                return RedirectToAction(nameof(Index));
            }
        }

        // ✅ POST: /hotel/book/{hotelId}
        [HttpPost("book/{hotelId}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Book(int hotelId, [FromForm] CreateReservationDto dto)
        {
            try
            {
                // Formdan gelen verileri doğrudan ReservationController'ın Create metoduna yönlendiriyoruz
                // Bu sayede kod tekrarı yapmamış oluruz.
                return RedirectToAction("Create", "Reservation", new { hotelId = hotelId, dto = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Book hatası: {hotelId}");
                TempData["ErrorMessage"] = "İşlem başlatılamadı.";
                return RedirectToAction(nameof(Details), new { id = hotelId });
            }
        }

        // ✅ GET: /hotel/search
        [HttpGet("search")]
        public async Task<IActionResult> Search(HotelSearchFilterDto dto)
        {
            try
            {
                var hotels = await _hotelService.FilterHotelsAsync(dto);
                ViewBag.City = dto.City;
                ViewBag.ResultCount = hotels?.Count ?? 0;
                return View("Index", hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelişmiş arama hatası.");
                return RedirectToAction(nameof(Index));
            }
        }

        // ✅ POST: /hotel/add-review
        [HttpPost("add-review")]
        [Authorize(Roles = "Guest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview([FromServices] StayHubContext context, int hotelId, int rating, string title, string comment)
        {
            try
            {
                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(guestIdStr, out int guestId)) return Unauthorized();

                var review = new Review
                {
                    HotelId = hotelId,
                    GuestId = guestId,
                    Rating = rating,
                    Title = title,
                    Comment = comment,
                    CreatedAt = DateTime.Now,
                    IsPublished = true
                };

                context.Reviews.Add(review);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Yorumunuz için teşekkürler!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum hatası.");
                TempData["ErrorMessage"] = "Yorum eklenemedi.";
            }

            return RedirectToAction("Details", new { id = hotelId });
        }
    }
}