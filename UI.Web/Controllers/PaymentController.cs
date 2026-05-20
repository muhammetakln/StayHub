using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Enum;
using Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UI.Web.Controllers
{
    [Route("payment")]
    [Authorize(Roles = "Guest")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IReservationService _reservationService;
        private readonly StayHubContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            IReservationService reservationService,
            StayHubContext context,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _reservationService = reservationService;
            _context = context;
            _logger = logger;
        }

        // ✅ GET: /payment/process/{reservationId}
        [HttpGet("process/{reservationId}")]
        public async Task<IActionResult> Process(int reservationId)
        {
            _logger.LogInformation($"Ödeme formu açılıyor: Reservation={reservationId}");

            var reservation = await _reservationService.GetReservationByIdAsync(reservationId);
            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Ödeme yapılacak rezervasyon kaydı bulunamadı.";
                return RedirectToAction("List", "Reservation");
            }

            var paymentDto = new PaymentProcessDto
            {
                OrderNumber = $"ORD-{reservationId}-{DateTime.Now.Ticks}",
                Amount = reservation.TotalPrice,
                Currency = "TRY"
            };

            ViewBag.ReservationId = reservationId;
            ViewBag.Amount = reservation.TotalPrice;
            ViewBag.CheckInDate = reservation.CheckInDate;
            ViewBag.CheckOutDate = reservation.CheckOutDate;

            return View(paymentDto);
        }

        // ✅ POST: /payment/process/{reservationId}
        [HttpPost("process/{reservationId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int reservationId, [FromForm] PaymentProcessDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen kart bilgilerini eksiksiz ve doğru doldurunuz.";
                return RedirectToAction("Process", new { reservationId });
            }

            // Ödeme servisini çağır (MockPaymentService)
            var result = await _paymentService.CreatePaymentAsync(reservationId, dto);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Process", new { reservationId });
            }

            return RedirectToAction("Success", new { reservationId });
        }

        // ✅ GET: /payment/success/{reservationId}
        [HttpGet("success/{reservationId}")]
        public async Task<IActionResult> Success(int reservationId)
        {
            _logger.LogInformation($"Başarı sayfası hazırlanıyor: Reservation={reservationId}");

            // Rezervasyonu mail gönderimi için ilişkileriyle çekiyoruz
            var reservation = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .ThenInclude(rm => rm.Hotel)
                .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

            if (reservation == null)
            {
                _logger.LogWarning($"Success sayfasında rezervasyon bulunamadı: {reservationId}");
                return RedirectToAction("List", "Reservation");
            }

            // Statüyü onayla ve faturayı ilk kez gönderiliyorsa tetikle
            if (reservation.Status != ReservationStatus.Confirmed)
            {
                reservation.Status = ReservationStatus.Confirmed;
                reservation.UpdatedAt = DateTime.UtcNow;
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                // 🚀 Şık tasarımlı faturayı gönder (Arka planda çalışması için)
                _ = _reservationService.SendInvoiceEmail(reservation.Guest, reservation, reservation.Room);
                _logger.LogInformation($"✅ Rezervasyon onaylandı ve fatura maili tetiklendi.");
            }

            var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);

            ViewBag.ReservationId = reservationId;
            ViewBag.Amount = reservation.TotalPrice;
            ViewBag.TransactionId = payment?.TransactionId ?? "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            ViewBag.HotelName = reservation.Room?.Hotel?.Name ?? "StayHub Hotel";
            ViewBag.CheckInDate = reservation.CheckInDate;
            ViewBag.CheckOutDate = reservation.CheckOutDate;

            return View();
        }

        // ✅ API: Ödeme Durum Kontrolü
        [HttpGet("status/{reservationId}")]
        public async Task<IActionResult> Status(int reservationId)
        {
            var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);
            if (payment == null) return Json(new { status = "NotFound" });

            return Json(new
            {
                status = payment.Status,
                amount = payment.Amount,
                transactionId = payment.TransactionId,
                paymentDate = payment.PaymentDate
            });
        }
    }
}