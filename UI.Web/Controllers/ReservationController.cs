using Business.Services;
using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Guest")] // Sadece Misafirler erişebilir
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
                if (reservation == null) return RedirectToAction(nameof(List));

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

                    AddOnServices = reservation.SelectedServices ?? new List<AddOnServiceDto>(),
                    GrandTotal = reservation.TotalPrice,
                };

                return View(detailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detay sayfası hatası");
                return RedirectToAction(nameof(List));
            }
        }

        // ✅ POST: /reservation/create/{hotelId}
        [HttpPost("create/{hotelId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int hotelId, CreateReservationDto dto)
        {
            try
            {
                var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(guestIdStr, out int guestId)) return Unauthorized();

                // ✅ ReservationService içindeki yeni mantık SelectedServiceIds listesini kullanacak
                await _reservationService.CreateReservationAsync(guestId, dto);

                TempData["SuccessMessage"] = "Rezervasyonunuz ve seçtiğiniz ek hizmetler başarıyla kaydedildi!";
                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyon oluşturma hatası");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Hotel", new { id = hotelId });
            }
        }

        // ✅ POST: /reservation/cancel/{id}
        [HttpPost("cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                _logger.LogInformation($"[CANCEL] Rezervasyon iptal süreci: {id}");

                var reservationDto = await _reservationService.GetReservationByIdAsync(id);
                if (reservationDto == null) return RedirectToAction(nameof(List));

                var paymentDto = await _paymentService.GetPaymentByReservationIdAsync(id);
                var cancelResult = await _reservationService.CancelReservationAsync(id);

                if (cancelResult.IsSuccess)
                {
                    if (paymentDto != null && paymentDto.Status == "Completed")
                    {
                        await _paymentService.ProcessRefundAsync(paymentDto.Id);
                    }

                    await _reservationService.SendCancellationEmail(id);
                    TempData["SuccessMessage"] = "Rezervasyon iptal edildi ve iade süreci başlatıldı.";
                }
                else
                {
                    TempData["ErrorMessage"] = cancelResult.Message;
                }

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İptal akışında hata");
                return RedirectToAction(nameof(List));
            }
        }
    }
}