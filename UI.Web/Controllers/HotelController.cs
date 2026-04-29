using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchTerm = null, string? city = null)
        {
            try
            {
                _logger.LogInformation("Hotel Index açılıyor");
                var hotels = await _hotelService.GetHotelsAsync();

                // Güvenli filtreleme (Null Check eklendi)
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    hotels = hotels
                        .Where(h => (h.Name != null && h.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                                    (h.City != null && h.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                                    (h.Description != null && h.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(city))
                {
                    hotels = hotels.Where(h => h.City != null && h.City.Equals(city, StringComparison.OrdinalIgnoreCase)).ToList();
                }

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

        // ✅ DÜZELTME: 404 hatasını önlemek için "id" opsiyonel yapıldı (id?)
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

        [HttpGet("search")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Search(HotelSearchDto dto)
        {
            try
            {
                _logger.LogInformation("Gelişmiş arama yapılıyor");
                var hotels = await _hotelService.FilterHotelsAsync(new HotelFilterDto
                {
                    Name = dto.City,
                    City = dto.City,
                    IsActive = true
                });

                return View("Index", hotels ?? new List<HotelDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search hatası");
                TempData["ErrorMessage"] = "Arama yapılırken hata oluştu";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}