using AutoMapper;
using Core.Abstracts.Interfaces;
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
        private readonly IMapper _mapper;
        private readonly ILogger<HotelController> _logger;

        public HotelController(IHotelService hotelService, IMapper mapper, ILogger<HotelController> logger)
        {
            _hotelService = hotelService;
            _mapper = mapper;
            _logger = logger;
        }

        // ✅ GET: /hotel - Ana sayfa (Login olmadan erişilebilir)
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchTerm = null, string? city = null)
        {
            try
            {
                _logger.LogInformation("Hotel Index açılıyor");
                var hotels = await _hotelService.GetHotelsAsync();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    hotels = hotels
                        .Where(h => h.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                   h.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                   (h.Description != null && h.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(city))
                {
                    hotels = hotels.Where(h => h.City.Equals(city, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                ViewBag.SearchTerm = searchTerm;
                ViewBag.City = city;
                ViewBag.ResultCount = hotels.Count;

                return View(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hotel Index hatası");
                TempData["ErrorMessage"] = "Otel listesi yüklenirken hata oluştu";
                return View(new List<HotelDto>());
            }
        }

        // ✅ GET: /hotel/details/{id} - Detay sayfası (Login olmadan erişilebilir)
        [HttpGet("details/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation($"Hotel detayı açılıyor: {id}");

                // GetHotelByIdAsync HotelDetailDto döndürüyor
                var hotelDetail = await _hotelService.GetHotelByIdAsync(id);

                if (hotelDetail == null)
                {
                    _logger.LogWarning($"Otel bulunamadı: {id}");
                    TempData["ErrorMessage"] = "Otel bulunamadı";
                    return RedirectToAction("Index");
                }

                _logger.LogInformation($"Hotel detayı başarıyla yüklendi: {hotelDetail.Name}");
                return View(hotelDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hotel Details hatası: {id}");
                TempData["ErrorMessage"] = "Otel detayı yüklenirken hata oluştu";
                return RedirectToAction("Index");
            }
        }

        // ✅ GET: /hotel/by-city/{city} - Şehire göre (Login olmadan erişilebilir)
        [HttpGet("by-city/{city}")]
        [AllowAnonymous]
        public async Task<IActionResult> ByCity(string city)
        {
            try
            {
                _logger.LogInformation($"Şehir filtresi: {city}");
                var hotels = await _hotelService.GetHotelsByCityAsync(city);
                ViewBag.City = city;
                ViewBag.ResultCount = hotels.Count;
                return View("Index", hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ByCity hatası: {city}");
                TempData["ErrorMessage"] = "Oteller yüklenirken hata oluştu";
                return RedirectToAction("Index");
            }
        }

        // ✅ GET: /hotel/top-rated - En yüksek puanlı (Login olmadan erişilebilir)
        [HttpGet("top-rated")]
        [AllowAnonymous]
        public async Task<IActionResult> TopRated()
        {
            try
            {
                _logger.LogInformation("Top rated oteller listeleniyor");
                var hotels = await _hotelService.GetHotelsByRatingAsync(4.0m);
                ViewBag.City = "En Yüksek Puanlı";
                ViewBag.ResultCount = hotels.Count;
                return View("Index", hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TopRated hatası");
                TempData["ErrorMessage"] = "Oteller yüklenirken hata oluştu";
                return RedirectToAction("Index");
            }
        }

        // ✅ GET: /hotel/special-offers - Özel teklifler (Login olmadan erişilebilir)
        [HttpGet("special-offers")]
        [AllowAnonymous]
        public async Task<IActionResult> SpecialOffers()
        {
            try
            {
                _logger.LogInformation("Özel teklifler listeleniyor");
                var allHotels = await _hotelService.GetHotelsAsync();
                var specialHotels = allHotels
                    .Where(h => h.HotelType == "Luxury" || h.HotelType == "Resort")
                    .ToList();

                ViewBag.City = "Özel Teklifler";
                ViewBag.ResultCount = specialHotels.Count;
                return View("Index", specialHotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SpecialOffers hatası");
                TempData["ErrorMessage"] = "Oteller yüklenirken hata oluştu";
                return RedirectToAction("Index");
            }
        }

        // 🔒 POST: /hotel/book - Rezervasyon (LOGIN GEREKLI!)
        [HttpPost("book/{hotelId}")]
        [Authorize(Roles = "Guest")] // ✅ SADECE Kayıtlı Guest'ler
        public async Task<IActionResult> Book(int hotelId, [FromForm] CreateReservationDto dto)
        {
            try
            {
                _logger.LogInformation($"Rezervasyon başlatıldı: Hotel={hotelId}");

                // TODO: ReservationService ile rezervasyon oluştur
                // var result = await _reservationService.CreateReservationAsync(guestId, dto);

                TempData["SuccessMessage"] = "Rezervasyon başarılı!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Book hatası: {hotelId}");
                TempData["ErrorMessage"] = "Rezervasyon yapılırken hata oluştu";
                return RedirectToAction("Details", new { id = hotelId });
            }
        }

        // 🔒 GET: /hotel/search - Gelişmiş arama (LOGIN GEREKLI!)
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
                return View("Index", hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search hatası");
                TempData["ErrorMessage"] = "Arama yapılırken hata oluştu";
                return RedirectToAction("Index");
            }
        }
    }
}