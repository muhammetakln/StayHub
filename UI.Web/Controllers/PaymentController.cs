using Business.Services;
using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enum;
using Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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

        // ✅ GET: /payment/process/{reservationId} - Ödeme Formu
        [HttpGet("process/{reservationId}")]
        public async Task<IActionResult> Process(int reservationId)
        {
            try
            {
                _logger.LogInformation($"Ödeme formu açılıyor: Reservation={reservationId}");

                var reservation = await _reservationService.GetReservationByIdAsync(reservationId);
                if (reservation == null)
                {
                    _logger.LogWarning($"Rezervasyon bulunamadı: {reservationId}");
                    TempData["ErrorMessage"] = "Rezervasyon bulunamadı";
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ödeme formu hatası: {reservationId}");
                TempData["ErrorMessage"] = "Ödeme formu yüklenirken hata oluştu";
                return RedirectToAction("List", "Reservation");
            }
        }

        // ✅ POST: /payment/process/{reservationId} - Ödeme İşle
        [HttpPost("process/{reservationId}")]
        public async Task<IActionResult> ProcessPayment(int reservationId, [FromForm] PaymentProcessDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Lütfen tüm alanları doğru doldurunuz";
                    return RedirectToAction("Process", new { reservationId });
                }

                var result = await _paymentService.CreatePaymentAsync(reservationId, dto);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Process", new { reservationId });
                }

                return RedirectToAction("Success", new { reservationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ödeme işleme hatası: {reservationId}");
                TempData["ErrorMessage"] = "Ödeme işlenirken hata oluştu.";
                return RedirectToAction("Process", new { reservationId });
            }
        }

        // ✅ GET: /payment/success/{reservationId} - Başarı ve Fatura Maili
        [HttpGet("success/{reservationId}")]
        public async Task<IActionResult> Success(int reservationId)
        {
            try
            {
                _logger.LogInformation($"Success süreci başlatıldı: Reservation={reservationId}");

                // ✅ 1. Rezervasyonu tüm ilişkileriyle birlikte çek (Mail için gerekli)
                var reservation = await _context.Reservations
                    .Include(r => r.Guest)
                    .Include(r => r.Room)
                    .ThenInclude(rm => rm.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                {
                    _logger.LogWarning($"Rezervasyon bulunamadı: {reservationId}");
                    return RedirectToAction("List", "Reservation");
                }

                // ✅ 2. Eğer daha önce onaylanmadıysa onaylanmış yap ve kaydet
                if (reservation.Status != ReservationStatus.Confirmed)
                {
                    reservation.Status = ReservationStatus.Confirmed;
                    reservation.UpdatedAt = DateTime.UtcNow;
                    _context.Reservations.Update(reservation);
                    await _context.SaveChangesAsync();

                    // ✅ 3. KRİTİK: Fatura Mailini Sadece İlk Onayda Gönder
                    // ReservationService içindeki public yaptığımız metodu çağırıyoruz
                    await _reservationService.SendInvoiceEmail(reservation.Guest, reservation, reservation.Room);
                    _logger.LogInformation($"✅ Fatura maili tetiklendi.");
                }

                var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);

                ViewBag.ReservationId = reservationId;
                ViewBag.Amount = reservation.TotalPrice;
                ViewBag.TransactionId = payment?.TransactionId ?? "TXN-AUTO-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                ViewBag.HotelName = reservation.Room?.Hotel?.Name ?? "StayHub Hotel";
                ViewBag.CheckInDate = reservation.CheckInDate;
                ViewBag.CheckOutDate = reservation.CheckOutDate;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Success sayfası/Fatura hatası: {reservationId}");
                return RedirectToAction("List", "Reservation");
            }
        }

        [HttpGet("status/{reservationId}")]
        public async Task<IActionResult> Status(int reservationId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);
                if (payment == null) return Json(new { status = "Not Found" });

                return Json(new
                {
                    status = payment.Status,
                    amount = payment.Amount,
                    transactionId = payment.TransactionId,
                    paymentDate = payment.PaymentDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Status kontrol hatası: {reservationId}");
                return Json(new { status = "Error", message = ex.Message });
            }
        }
    }
}