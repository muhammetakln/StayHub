using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UI.Web.Controllers
{
    [Route("hotel")]
    public class HotelController : Controller
    {
        private readonly IHotelService _hotelService;
        private readonly IReviewService _reviewService;
        private readonly IReservationService _reservationService;
        private readonly ILogger<HotelController> _logger;

        public HotelController(
            IHotelService hotelService,
            IReviewService reviewService,
            IReservationService reservationService,
            ILogger<HotelController> logger)
        {
            _hotelService = hotelService;
            _reviewService = reviewService;
            _reservationService = reservationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? city, string? searchTerm)
        {
           
            ModelState.Clear();

            var filter = new HotelSearchFilterDto
            {
                City = city,
                SearchKeyword = searchTerm,
                PageNumber = 1,
                PageSize=200
            };

            var hotels = await _hotelService.FilterHotelsAsync(filter);

            // Değerler boşsa null set ederek arayüzün temiz kalmasını sağlıyoruz
            ViewBag.City = string.IsNullOrWhiteSpace(city) ? null : city;
            ViewBag.SearchKeyword = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm;
            ViewBag.ResultCount = hotels.Count;

            return View(hotels);
        }

        [HttpGet("details/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0) return RedirectToAction(nameof(Index));
                var hotelDetail = await _hotelService.GetHotelByIdAsync(id);
                if (hotelDetail == null) return RedirectToAction(nameof(Index));

                return View(hotelDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detay sayfası hatası.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("add-review")]
        [Authorize(Roles = "Guest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int hotelId, int rating, string title, string Content)
        {
            try
            {
                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(guestIdStr, out int guestId))
                    return Unauthorized(new { message = "Giriş yapmalısınız." });

                var result = await _reviewService.AddReviewAsync(hotelId, guestId, rating, title, Content);

                if (result.IsSuccess)
                    return Ok(new { success = true, message = "Yorum eklendi." });
                else
                    return StatusCode(500, new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum ekleme hatası.");
                return StatusCode(500, new { success = false });
            }
        }

       
    }
}