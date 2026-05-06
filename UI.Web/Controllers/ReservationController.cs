using Business.Services;
using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UI.Web.Controllers
{
    [Route("reservation")]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IHotelService _hotelService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IReservationService reservationService,
            IHotelService hotelService,
            ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _hotelService = hotelService;
            _logger = logger;
        }

        // ✅ POST: /reservation/create/{hotelId} - Yeni Rezervasyon Oluştur
        // ✅ POST: /reservation/create/{hotelId} - Yeni Rezervasyon Oluştur
        [HttpPost("create/{hotelId}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Create(int hotelId, [FromForm] CreateReservationDto dto)
        {
            try
            {
                _logger.LogInformation($"Rezervasyon başlatıldı: Hotel={hotelId}, Guest={User.FindFirst(ClaimTypes.NameIdentifier)?.Value}");

                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(guestIdStr) || !int.TryParse(guestIdStr, out int guestId))
                {
                    _logger.LogWarning("Guest ID alınamadı");
                    TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı";
                    return RedirectToAction("Details", "Hotel", new { id = hotelId });
                }

                var hotelExists = await _hotelService.IsHotelExistsAsync(hotelId);
                if (!hotelExists)
                {
                    _logger.LogWarning($"Otel bulunamadı: {hotelId}");
                    TempData["ErrorMessage"] = "Otel bulunamadı";
                    return RedirectToAction("Index", "Hotel");
                }

                dto.GuestId = guestId;

                var reservations = await _reservationService.CreateReservationAsync(guestId, dto);

                _logger.LogInformation($"Rezervasyon başarıyla oluşturuldu");

                TempData["SuccessMessage"] = $"✅ Rezervasyon başarılı!";

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon oluşturma hatası: Hotel={hotelId}");

                // DEĞİŞİKLİK BURADA: Statik metin yerine servisten dönen gerçek hatayı (ex.Message) alıyoruz.
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction("Details", "Hotel", new { id = hotelId });
            }
        }
        // ✅ GET: /reservation/details/{id} - Rezervasyon Detayları
        [HttpGet("details/{id}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation($"Rezervasyon detayları alınıyor: {id}");

                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null)
                {
                    _logger.LogWarning($"Rezervasyon bulunamadı: {id}");
                    TempData["ErrorMessage"] = "Rezervasyon bulunamadı";
                    return RedirectToAction("List");
                }

                // ✅ Tarihlerden gece sayısını hesapla
                var nightCount = (int)(reservation.CheckOutDate - reservation.CheckInDate).TotalDays;

                // ✅ Manual mapping - ReservationDto'dan ReservationDetailDto oluştur
                var detailDto = new ReservationDetailDto
                {
                    Id = reservation.Id,
                    HotelId = reservation.HotelId,
                    HotelName = reservation.HotelName,
                    HotelAddress = "Address",  // TODO: Service'den gelecek
                    HotelPhone = "Phone",  // TODO: Service'den gelecek
                    RoomId = reservation.RoomId,
                    RoomNumber = reservation.RoomNumber,
                    RoomType = "Standard",  // TODO: Service'den gelecek
                    CheckInDate = reservation.CheckInDate,
                    CheckOutDate = reservation.CheckOutDate,
                    NightCount = nightCount,
                    Status = reservation.Status,
                    SpecialRequests = null,
                    PricePerNight = reservation.TotalPrice / (nightCount > 0 ? nightCount : 1),
                    SubTotal = reservation.TotalPrice,
                    Tax = 0,
                    AddOnServices = new List<AddOnServiceDto>(),
                    AddOnTotal = 0,
                    GrandTotal = reservation.TotalPrice,
                    CreatedAt = DateTime.Now
                };

                return View(detailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon detayları hatası: {id}");
                TempData["ErrorMessage"] = "Rezervasyon detayları yüklenirken hata oluştu";
                return RedirectToAction("List");
            }
        }

        // ✅ GET: /reservation/list - Kullanıcının Tüm Rezervasyonları
        [HttpGet("list")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> List()
        {
            try
            {
                _logger.LogInformation($"Rezervasyonlar listeleniyor: Guest={User.FindFirst(ClaimTypes.NameIdentifier)?.Value}");

                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(guestIdStr) || !int.TryParse(guestIdStr, out int guestId))
                {
                    _logger.LogWarning("Guest ID alınamadı");
                    return Unauthorized();
                }

                var reservations = await _reservationService.GetReservationsByIdAsync(guestId);

                _logger.LogInformation($"Toplam {reservations.Count} rezervasyon getirildi");
                return View(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyonlar listesi hatası");
                TempData["ErrorMessage"] = "Rezervasyonlar yüklenirken hata oluştu";
                return RedirectToAction("Index", "Hotel");
            }
        }

        // ✅ POST: /reservation/cancel/{id} - Rezervasyonu İptal Et
        [HttpPost("cancel/{id}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                _logger.LogInformation($"Rezervasyon iptal edilecek: {id}");

                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null)
                {
                    _logger.LogWarning($"Rezervasyon bulunamadı: {id}");
                    TempData["ErrorMessage"] = "Rezervasyon bulunamadı";
                    return RedirectToAction("List");
                }

                await _reservationService.CancelReservationAsync(id);

                _logger.LogInformation($"Rezervasyon başarıyla iptal edildi: {id}");
                TempData["SuccessMessage"] = "Rezervasyon başarıyla iptal edildi";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon iptal hatası: {id}");
                TempData["ErrorMessage"] = "Rezervasyon iptal edilirken hata oluştu";
                return RedirectToAction("Details", new { id = id });
            }
        }
    }
}