using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

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
            // ✅ PROFESYONEL DOKUNUŞ: Tarayıcı ve sunucu tarafındaki form geçmişi çakışmalarını
            // engellemek için ModelState temizlenir. Bu sayede farklı hesap girişlerinde
            // eski filtre değerleri kutucuklarda asılı kalmaz.
            ModelState.Clear();

            var filter = new HotelSearchFilterDto
            {
                City = city,
                SearchKeyword = searchTerm,
                PageNumber = 1,
                PageSize = 50
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

        [HttpPost("delete-review/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var result = await _reviewService.DeleteReviewAsync(id);
            if (!result.IsSuccess) return NotFound();

            return Ok(new { success = true });
        }

        [HttpPost("reply-review")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ReplyReview(int reviewId, string replyText)
        {
            var result = await _reviewService.ReplyReviewAsync(reviewId, replyText);
            if (!result.IsSuccess) return NotFound();

            return Ok(new { success = true });
        }

        [HttpGet("get-revenue/{hotelId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetRevenue(int hotelId)
        {
            var revenue = await _reservationService.GetMonthlyRevenueByHotelIdAsync(hotelId);
            return Ok(new { hotelId, monthlyRevenue = revenue });
        }

        [HttpPost("book/{hotelId}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Book(int hotelId, [FromForm] CreateReservationDto dto)
        {
            return RedirectToAction("Create", "Reservation", new { hotelId = hotelId, dto = dto });
        }
    }
}