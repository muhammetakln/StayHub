using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Guest")]
    [Route("reservation")]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IReservationService reservationService,
            IPaymentService paymentService,
            ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _paymentService = paymentService;
            _logger = logger;
        }

        // ✅ GET: /reservation/list
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(guestIdStr, out int guestId)) return Unauthorized();

            var reservations = await _reservationService.GetReservationsByIdAsync(guestId);
            _logger.LogInformation($"[LIST] Guest={guestId}, Count={reservations.Count}");

            return View(reservations);
        }

        // ✅ GET: /reservation/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
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

        // ✅ POST: /reservation/create/{hotelId}
        [HttpPost("create/{hotelId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int hotelId, CreateReservationDto dto)
        {
            var guestIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(guestIdStr, out int guestId)) return Unauthorized();

            // ✅ IResult<List<ReservationDto>> yapısına göre güncellendi
            var result = await _reservationService.CreateReservationAsync(guestId, dto);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Details", "Hotel", new { id = hotelId });
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(List));
        }

        // ✅ POST: /reservation/cancel/{id}
        [HttpPost("cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            _logger.LogInformation($"[CANCEL] Rezervasyon iptal süreci: {id}");

            var paymentDto = await _paymentService.GetPaymentByReservationIdAsync(id);

            var cancelResult = await _reservationService.CancelReservationAsync(id);

            if (cancelResult.IsSuccess)
            {
                if (paymentDto != null && paymentDto.Status == "Completed")
                {
                    await _paymentService.ProcessRefundAsync(paymentDto.Id);
                }

                await _reservationService.SendCancellationEmail(id);
                TempData["SuccessMessage"] = cancelResult.Message;
            }
            else
            {
                TempData["ErrorMessage"] = cancelResult.Message;
            }

            return RedirectToAction(nameof(List));
        }
    }
}