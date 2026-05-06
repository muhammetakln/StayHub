using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities; // ✅ Review nesnesi için eklendi
using Data.Contexts; // ✅ StayHubContext için eklendi
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // ✅ Kullanıcı ID'sini almak için eklendi
using System.Threading.Tasks;

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

        // ✅ DÜZELTME: Bütün otelleri belleğe çekmek yerine doğrudan FilterHotelsAsync ile veritabanında arama yapılır.
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchTerm = null, string? city = null)
        {
            try
            {
                _logger.LogInformation("Hotel Index açılıyor");

                // Arama parametrelerini oluştur
                var filter = new HotelSearchFilterDto
                {
                    SearchKeyword = searchTerm,
                    City = city,
                    PageSize = 50 // Maksimum 50 otel gösterilsin
                };

                // Doğrudan veritabanı seviyesinde filtrelenmiş veriyi al
                var hotels = await _hotelService.FilterHotelsAsync(filter);

                ViewBag.SearchTerm = searchTerm;
                ViewBag.City = city;
                ViewBag.ResultCount = hotels?.Count ?? 0;

                return View(hotels ?? new List<HotelDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hotel Index hatası");
                TempData["ErrorMessage"] = "Otel listesi yüklenirken hata oluştu";
                return View(new List<HotelDto>());
            }
        }

        [HttpGet("details/{id?}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                // Eğer URL'ye ID girilmeden gelinirse, çökmek yerine ana sayfaya atar
                if (id == null || id <= 0)
                {
                    _logger.LogWarning("Geçersiz veya eksik ID ile detay sayfasına erişilmeye çalışıldı.");
                    TempData["ErrorMessage"] = "Lütfen incelemek istediğiniz oteli listeden seçin.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation($"Hotel detayı açılıyor: {id}");

                var hotelDetail = await _hotelService.GetHotelByIdAsync(id.Value);

                if (hotelDetail == null)
                {
                    _logger.LogWarning($"Otel bulunamadı: {id}");
                    TempData["ErrorMessage"] = "Aradığınız otel bulunamadı veya yayından kaldırılmış olabilir.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation($"Hotel detayı başarıyla yüklendi: {hotelDetail.Name}");
                return View(hotelDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hotel Details hatası: {id}");
                TempData["ErrorMessage"] = "Otel detayı yüklenirken sistemsel bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("by-city/{city}")]
        [AllowAnonymous]
        public async Task<IActionResult> ByCity(string city)
        {
            try
            {
                _logger.LogInformation($"Şehir filtresi: {city}");
                var hotels = await _hotelService.GetHotelsByCityAsync(city);

                ViewBag.City = city;
                ViewBag.ResultCount = hotels?.Count ?? 0;

                return View("Index", hotels ?? new List<HotelDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ByCity hatası: {city}");
                TempData["ErrorMessage"] = "Oteller yüklenirken hata oluştu";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("top-rated")]
        [AllowAnonymous]
        public async Task<IActionResult> TopRated()
        {
            try
            {
                _logger.LogInformation("Top rated oteller listeleniyor");
                var hotels = await _hotelService.GetHotelsByRatingAsync(4.0m);

                ViewBag.City = "En Yüksek Puanlı";
                ViewBag.ResultCount = hotels?.Count ?? 0;

                return View("Index", hotels ?? new List<HotelDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TopRated hatası");
                TempData["ErrorMessage"] = "Oteller yüklenirken hata oluştu";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("special-offers")]
        [AllowAnonymous]
        public async Task<IActionResult> SpecialOffers()
        {
            try
            {
                _logger.LogInformation("Özel teklifler listeleniyor");
                var allHotels = await _hotelService.GetHotelsAsync();

                var specialHotels = allHotels?
                    .Where(h => h.HotelType == "Luxury" || h.HotelType == "Resort")
                    .ToList() ?? new List<HotelDto>();

                ViewBag.City = "Özel Teklifler";
                ViewBag.ResultCount = specialHotels.Count;

                return View("Index", specialHotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SpecialOffers hatası");
                TempData["ErrorMessage"] = "Oteller yüklenirken hata oluştu";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("book/{hotelId}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Book(int hotelId, [FromForm] CreateReservationDto dto)
        {
            try
            {
                _logger.LogInformation($"Rezervasyon başlatıldı: Hotel={hotelId}");
                TempData["SuccessMessage"] = "Rezervasyon talebiniz başarıyla alındı!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Book hatası: {hotelId}");
                TempData["ErrorMessage"] = "Rezervasyon yapılırken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Details), new { id = hotelId });
            }
        }

        // ✅ DÜZELTME: Eski HotelFilterDto özelliklerini kullanmaya çalışan atamalar temizlendi.
        [HttpGet("search")]
        [AllowAnonymous] // Herkes arama yapabilsin
        public async Task<IActionResult> Search(HotelSearchFilterDto dto)
        {
            try
            {
                _logger.LogInformation("Gelişmiş arama yapılıyor");

                // Formdan (View'dan) gelen dto'yu doğrudan kullanarak filtreleme yapıyoruz
                var hotels = await _hotelService.FilterHotelsAsync(dto);

                ViewBag.SearchTerm = dto.SearchKeyword;
                ViewBag.City = dto.City;
                ViewBag.ResultCount = hotels?.Count ?? 0;

                return View("Index", hotels ?? new List<HotelDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search hatası");
                TempData["ErrorMessage"] = "Arama yapılırken hata oluştu";
                return RedirectToAction(nameof(Index));
            }
        }

        // ✅ YENİ EKLENDİ: Yorum Yapma Metodu
        [HttpPost("add-review")]
        [Authorize(Roles = "Guest")] // Sadece giriş yapmış misafirler
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview([FromServices] StayHubContext context, int hotelId, int rating, string title, string comment)
        {
            try
            {
                // Kullanıcının ID'sini alıyoruz
                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(guestIdStr) || !int.TryParse(guestIdStr, out int guestId))
                {
                    TempData["ErrorMessage"] = "Yorum yapmak için giriş yapmalısınız.";
                    return RedirectToAction("Details", new { id = hotelId });
                }

                // Yeni yorum nesnesini oluşturuyoruz
                var review = new Review
                {
                    HotelId = hotelId,
                    GuestId = guestId,
                    Rating = rating,
                    Title = title,
                    Comment = comment,
                    CreatedAt = DateTime.Now,
                    IsPublished = true,
                    IsDeleted = false,
                    HelpfulCount = 0,
                    UnhelpfulCount = 0,
                    IsReplied = false
                };

                // Veritabanına kaydet
                context.Reviews.Add(review);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Yorumunuz başarıyla eklendi, teşekkür ederiz!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum eklenirken hata oluştu.");
                TempData["ErrorMessage"] = "Yorumunuz eklenirken beklenmedik bir hata oluştu.";
            }

            // İşlem bitince kullanıcıyı tekrar otel detay sayfasına gönder
            return RedirectToAction("Details", new { id = hotelId });
        }
    }
}