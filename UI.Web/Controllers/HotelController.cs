using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
        public async Task<IActionResult> Index(string? city)
        {
            List<HotelDto> hotels;
            if (!string.IsNullOrWhiteSpace(city))
            {
                hotels = await _hotelService.GetHotelsByCityAsync(city);
                ViewBag.City = city;
                ViewBag.ResultCount = hotels.Count;
            }
            else
            {
                hotels = await _hotelService.GetHotelsAsync();
                ViewBag.City = null;
            }
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
        public async Task<IActionResult> AddReview([FromServices] StayHubContext context, int hotelId, int rating, string title, string Content)
        {
            try
            {
                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(guestIdStr, out int guestId))
                    return Unauthorized(new { message = "Giriş yapmalısınız." });

                var review = new Review
                {
                    HotelId = hotelId,
                    GuestId = guestId,
                    Rating = rating,
                    Title = title,
                    Comment = Content,
                    CreatedAt = DateTime.Now,
                    IsPublished = true,
                    IsDeleted = false
                };

                context.Reviews.Add(review);
                await context.SaveChangesAsync();

                return Ok(new { success = true, message = "Yorum eklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false });
            }
        }

        // ✅ YENİ: YORUM SİLME (Admin/SuperAdmin)
        [HttpPost("delete-review/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteReview([FromServices] StayHubContext context, int id)
        {
            var review = await context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.IsDeleted = true; // Soft delete
            await context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ✅ YENİ: YORUMA YANIT VERME (Admin/SuperAdmin)
        [HttpPost("reply-review")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ReplyReview([FromServices] StayHubContext context, int reviewId, string replyText)
        {
            var review = await context.Reviews.FindAsync(reviewId);
            if (review == null) return NotFound();

            review.OwnerReply = replyText;
            review.OwnerReplyDate = DateTime.Now;
            review.IsReplied = true;

            await context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ✅ YENİ: CİRO HESAPLAMA (Admin Paneli İçin)
        [HttpGet("get-revenue/{hotelId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetRevenue([FromServices] StayHubContext context, int hotelId)
        {
            // Son 30 gündeki onaylanmış (Confirmed) rezervasyonların toplamı
            var revenue = await context.Reservations
                .Where(r => r.Room.HotelId == hotelId &&
                            r.Status == Core.Concretes.Enum.ReservationStatus.Confirmed &&
                            !r.IsDeleted &&
                            r.CreatedAt >= DateTime.Now.AddDays(-30))
                .SumAsync(r => r.TotalPrice);

            return Ok(new { hotelId, monthlyRevenue = revenue });
        }

        [HttpPost("book/{hotelId}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Book(int hotelId, [FromForm] CreateReservationDto dto)
        {
            return RedirectToAction("Create", "Reservation", new { hotelId = hotelId, dto = dto });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(HotelSearchFilterDto dto)
        {
            var hotels = await _hotelService.FilterHotelsAsync(dto);
            return View("Index", hotels);
        }
    }
}