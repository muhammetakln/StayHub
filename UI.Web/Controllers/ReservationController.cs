using Business.Services;
using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Guest")] // Tüm controller için yetki şartı
    [Route("reservation")]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IHotelService _hotelService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IReservationService reservationService,
            IHotelService hotelService,
            IPaymentService paymentService,
            ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _hotelService = hotelService;
            _paymentService = paymentService;
            _logger = logger;
        }

        // ✅ GET: /reservation/list
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            try
            {
                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(guestIdStr, out int guestId)) return Unauthorized();

                var reservations = await _reservationService.GetReservationsByIdAsync(guestId);
                _logger.LogInformation($"[LIST] Guest={guestId}, Count={reservations.Count}");

                return View(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyon listesi yüklenemedi");
                return RedirectToAction("Index", "Hotel");
            }
        }

        // ✅ GET: /reservation/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null) return RedirectToAction("List");

                var nightCount = (int)(reservation.CheckOutDate - reservation.CheckInDate).TotalDays;

                var detailDto = new ReservationDetailDto
                {
                    Id = reservation.Id,
                    HotelName = reservation.HotelName,
                    RoomNumber = reservation.RoomNumber,
                    RoomType = reservation.RoomName ?? "Standart Oda",
                    CheckInDate = reservation.CheckInDate,
                    CheckOutDate = reservation.CheckOutDate,
                    NightCount = nightCount,
                    Status = reservation.Status,
                    PricePerNight = reservation.TotalPrice / (nightCount > 0 ? nightCount : 1),
                    AddOnServices = reservation.SelectedServices,
                    GrandTotal = reservation.TotalPrice,
                    CreatedAt = DateTime.Now
                };

                return View(detailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detay sayfası hatası");
                return RedirectToAction("List");
            }
        }

        // ✅ POST: /reservation/create/{hotelId}
        [HttpPost("create/{hotelId}")]
        public async Task<IActionResult> Create(int hotelId, [FromForm] CreateReservationDto dto)
        {
            try
            {
                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(guestIdStr, out int guestId)) return Unauthorized();

                await _reservationService.CreateReservationAsync(guestId, dto);
                TempData["SuccessMessage"] = "Rezervasyon başarıyla oluşturuldu!";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Hotel", new { id = hotelId });
            }
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                _logger.LogInformation($"[CANCEL] Rezervasyon iptal süreci: {id}");

                // 1. Rezervasyonu DTO olarak çek (Otel adı, misafir adı vb. bilgiler içinde gelir)
                var reservationDto = await _reservationService.GetReservationByIdAsync(id);
                if (reservationDto == null) return RedirectToAction("List");

                // 2. Ödeme bilgisini al (İptal edilmeden önce kontrol etmeliyiz)
                var paymentDto = await _paymentService.GetPaymentByReservationIdAsync(id);

                // 3. Rezervasyonu iptal et
                var cancelResult = await _reservationService.CancelReservationAsync(id);

                if (cancelResult.IsSuccess)
                {
                    // 4. Ödeme varsa iade işlemini başlat
                    if (paymentDto != null && paymentDto.Status == "Completed")
                    {
                        await _paymentService.ProcessRefundAsync(paymentDto.Id);
                    }

                    // 5. Bilgilendirme maili gönder
                    // Not: ReservationService içinde SendCancellationEmail metoduna 
                    // rezervasyonun ID'sini göndererek entity'e içeride ulaşmasını sağlıyoruz.
                    await _reservationService.SendCancellationEmail(id);

                    TempData["SuccessMessage"] = "Rezervasyon iptal edildi ve bilgilendirme maili gönderildi.";
                }
                else
                {
                    TempData["ErrorMessage"] = cancelResult.Message;
                }

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İptal akışında hata");
                return RedirectToAction("List");
            }
        }
    }
}
