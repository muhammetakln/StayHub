using Business.Services;
using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
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

                // Payment formu için DTO oluştur
                var paymentDto = new PaymentProcessDto
                {
                    OrderNumber = $"ORD-{reservationId}-{DateTime.Now.Ticks}",
                    Amount = reservation.TotalPrice,
                    Currency = "TRY"
                };

                // ViewBag'e rezervasyon bilgisi ekle
                ViewBag.ReservationId = reservationId;
                ViewBag.HotelName = "Hotel";  // TODO: Service'den çek
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
                _logger.LogInformation($"Ödeme işleniyor: Reservation={reservationId}, Bank={dto.PaymentMethod}");

                // ✅ Validation hatalarını kontrol et
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"[VALIDATION ERROR] {error.ErrorMessage}");
                    }

                    _logger.LogWarning("Model validation başarısız");
                    TempData["ErrorMessage"] = "Lütfen tüm alanları doğru doldurunuz";
                    return RedirectToAction("Process", new { reservationId });
                }

                // ✅ Ödemeyi işle
                var result = await _paymentService.CreatePaymentAsync(reservationId, dto);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning($"Ödeme başarısız: {result.Message}");
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Process", new { reservationId });
                }

                // ✅ Başarılı ödeme → Success sayfasına git
                _logger.LogInformation($"✅ Ödeme başarılı: Reservation={reservationId}");
                TempData["SuccessMessage"] = "✅ Ödeme yapıldı! Rezervasyonunuz onaylandı.";

                return RedirectToAction("Success", new { reservationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ödeme işleme hatası: {reservationId}");
                TempData["ErrorMessage"] = "Ödeme işlenirken hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction("Process", new { reservationId });
            }
        }

        // ✅ GET: /payment/success/{reservationId} - Başarı Sayfası
        [HttpGet("success/{reservationId}")]
        public async Task<IActionResult> Success(int reservationId)
        {
            try
            {
                _logger.LogInformation($"Success sayfası açılıyor: Reservation={reservationId}");

                // ✅ Entity'yi direkt DB'den çek
                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                {
                    _logger.LogWarning($"Rezervasyon bulunamadı: {reservationId}");
                    return RedirectToAction("List", "Reservation");
                }

                // ✅ Status'u Confirmed olarak güncelle
                reservation.Status = ReservationStatus.Confirmed;
                reservation.UpdatedAt = DateTime.UtcNow;

                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Reservation status güncellendi: Confirmed");

                // Payment bilgisini al
                var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);

                ViewBag.ReservationId = reservationId;
                ViewBag.Amount = reservation.TotalPrice;
                ViewBag.TransactionId = payment?.TransactionId ?? "TXN-MOCK-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                ViewBag.HotelName = "Hotel"; // TODO: Service'den çek
                ViewBag.CheckInDate = reservation.CheckInDate;
                ViewBag.CheckOutDate = reservation.CheckOutDate;

                _logger.LogInformation($"✅ Success sayfası render ediliyor");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Success sayfası hatası: {reservationId}");
                return RedirectToAction("List", "Reservation");
            }
        }

        // ✅ GET: /payment/status/{reservationId} - Ödeme Durumu
        [HttpGet("status/{reservationId}")]
        public async Task<IActionResult> Status(int reservationId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);
                if (payment == null)
                {
                    return Json(new { status = "Not Found", message = "Ödeme bulunamadı" });
                }

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