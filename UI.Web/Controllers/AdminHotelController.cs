using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;

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
        private readonly StayHubContext _context;

        public AdminHotelController(
            IHotelService hotelService,
            UserManager<Guest> userManager,
            SignInManager<Guest> signInManager,
            ILogger<AdminHotelController> logger,
            StayHubContext context)
        {
            _hotelService = hotelService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
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

                        // ✅ DİNAMİK VERİ HESAPLAMALARI (SQLite & Format Uyumlu)
                        var today = DateTime.Today;
                        var tomorrow = today.AddDays(1);

                        // 1. Aylık Ciro (SQLite decimal SUM hatası için ToList ile bellek üzerinden hesaplama)
                        var reservations = await _context.Reservations
                            .Where(r => r.Room.HotelId == user.HotelId &&
                                        r.Status == Core.Concretes.Enum.ReservationStatus.Confirmed &&
                                        r.CreatedAt >= DateTime.Now.AddDays(-30) && !r.IsDeleted)
                            .Select(r => r.TotalPrice)
                            .ToListAsync();
                        hotelDetail.MonthlyEarning = reservations.Sum();

                        // 2. Bugün Giriş Bekleyenler (Güvenli tarih aralığı sorgusu)
                        hotelDetail.TodayCheckIns = await _context.Reservations
                            .CountAsync(r => r.Room.HotelId == user.HotelId &&
                                             r.CheckInDate >= today && r.CheckInDate < tomorrow &&
                                             !r.IsDeleted);

                        // 3. Şu An Otelde Konaklayanlar (Status: CheckedIn)
                        hotelDetail.ActiveReservations = await _context.Reservations
                            .CountAsync(r => r.Room.HotelId == user.HotelId &&
                                             r.Status == Core.Concretes.Enum.ReservationStatus.CheckedIn &&
                                             !r.IsDeleted);

                        // 4. Misafir Puanı ve Toplam Yorum Sayısı (Anlık Hesaplama)
                        var ratings = await _context.Reviews
                            .Where(r => r.HotelId == user.HotelId && !r.IsDeleted)
                            .Select(r => r.Rating)
                            .ToListAsync();

                        hotelDetail.ReviewCount = ratings.Count;
                        hotelDetail.AverageRating = ratings.Any() ? Math.Round(ratings.Average(), 1) : 0;

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

        // ✅ YORUM SİLME (AJAX)
        [HttpPost("delete-review/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ✅ YORUMA YANIT VERME (AJAX)
        [HttpPost("reply-review")]
        public async Task<IActionResult> ReplyReview(int reviewId, string replyText)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return NotFound();

            review.OwnerReply = replyText;
            review.OwnerReplyDate = DateTime.Now;
            review.IsReplied = true;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
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
        public async Task<IActionResult> Create(CreateHotelDto dto, List<IFormFile> HotelImages)
        {
            try
            {
                if (!ModelState.IsValid) return View(dto);

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                int createdHotelId = await _hotelService.CreateHotelAsync(dto);

                if (createdHotelId > 0 && HotelImages != null && HotelImages.Count > 0)
                {
                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "hotels");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    foreach (var file in HotelImages)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }

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
        public async Task<IActionResult> Edit(int id, UpdateHotelDto dto, List<IFormFile> HotelImages)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && (user == null || user.HotelId != id)) return Forbid();

            try
            {
                await _hotelService.UpdateHotelAsync(id, dto);
                TempData["SuccessMessage"] = "Otel bilgileri başarıyla güncellendi!";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel güncellenirken hata oluştu.");
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