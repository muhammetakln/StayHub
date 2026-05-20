using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enum;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utils.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services
{
    public class MockPaymentService : IPaymentService
    {
        private readonly StayHubContext _context;
        private readonly ILogger<MockPaymentService> _logger;
        private readonly IReservationService _reservationService; // 👈 1. Rezervasyon servisi bağımlılığı eklendi

        private static readonly Dictionary<string, PaymentStatus> MockDatabase = new();

        // Constructor güncellenerek IReservationService enjekte edildi
        public MockPaymentService(StayHubContext context, ILogger<MockPaymentService> logger, IReservationService reservationService)
        {
            _context = context;
            _logger = logger;
            _reservationService = reservationService; // 👈 2. Ataması yapıldı
        }

        public async Task<IResult> CreatePaymentAsync(int reservationId, PaymentProcessDto dto)
        {
            try
            {
                _logger.LogInformation($"[MOCK PAYMENT] Ödeme oluşturuluyor: Reservation={reservationId}");

                // 🎯 GÜNCELLENDİ: Nesne kilitlenmesini engellemek için .AsNoTracking() eklendi
                var reservation = await _context.Reservations
                    .AsNoTracking()
                    .Include(r => r.Guest)
                    .Include(r => r.Room)
                        .ThenInclude(rm => rm.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                {
                    _logger.LogWarning($"[MOCK PAYMENT] Rezervasyon bulunamadı: {reservationId}");
                    return Result.Failure("Rezervasyon bulunamadı");
                }

                if (string.IsNullOrEmpty(dto.CardNumber) || dto.CardNumber.Length < 13)
                {
                    _logger.LogWarning($"[MOCK PAYMENT] Geçersiz kart numarası");
                    return Result.Failure("Geçersiz kart numarası");
                }

                var bankStatus = SimulateBank(dto.PaymentMethod ?? "garanti", dto.Amount);

                var payment = new Payment
                {
                    ReservationId = reservationId,
                    PaymentReference = $"PAY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Amount = dto.Amount,
                    PaymentMethod = dto.PaymentMethod ?? "garanti",
                    Status = bankStatus == "Completed" ? PaymentStatus.Completed : PaymentStatus.Failed,
                    TransactionId = $"TXN-{Guid.NewGuid().ToString().Substring(0, 12).ToUpper()}",
                    Notes = dto.Description,
                    PaymentDate = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[MOCK PAYMENT] Ödeme oluşturuldu: ID={payment.Id}, Status={payment.Status}");

                // 🎯 3. FATURA MAİLİ BURADA TETİKLENİYOR
                if (payment.Status == PaymentStatus.Completed && reservation.Guest != null && reservation.Room != null)
                {
                    // 🎯 GÜNCELLENDİ: AsNoTracking kullandığımız için rezervasyon durumunu veritabanından bulup temiz bir şekilde güncelliyoruz
                    var trackableReservation = await _context.Reservations.FindAsync(reservationId);
                    if (trackableReservation != null)
                    {
                        trackableReservation.Status = ReservationStatus.Confirmed;
                        _context.Reservations.Update(trackableReservation);
                        await _context.SaveChangesAsync();
                    }

                    _logger.LogInformation($"[MOCK PAYMENT] Ödeme başarılı. Fatura maili gönderiliyor: Reservation={reservationId}");

                    // Önceki adımda ReservationService içinde bıraktığımız fatura metodunu çağırıyoruz
                    await _reservationService.SendInvoiceEmail(reservation.Guest, reservation, reservation.Room);
                }

                return Result.Success($"Ödeme {(bankStatus == "Completed" ? "başarılı" : "başarısız")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOCK PAYMENT] CreatePaymentAsync hatası");
                return Result.Failure("Ödeme oluşturulurken hata oluştu");
            }
        }

        public async Task<PaymentDetailDto?> GetPaymentByReservationIdAsync(int reservationId)
        {
            try
            {
                _logger.LogInformation($"[MOCK PAYMENT] Ödeme alınıyor: Reservation={reservationId}");

                var payment = await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ReservationId == reservationId && !p.IsDeleted);

                if (payment == null)
                {
                    _logger.LogWarning($"[MOCK PAYMENT] Ödeme bulunamadı: {reservationId}");
                    return null;
                }

                return new PaymentDetailDto
                {
                    Id = payment.Id,
                    ReservationId = payment.ReservationId,
                    OrderNumber = payment.PaymentReference,
                    Amount = payment.Amount,
                    Currency = "TRY",
                    Status = payment.Status.ToString(),
                    PaymentMethod = payment.PaymentMethod,
                    TransactionId = payment.TransactionId,
                    PaymentDate = payment.PaymentDate,
                    Description = payment.Notes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOCK PAYMENT] GetPaymentByReservationIdAsync hatası");
                return null;
            }
        }

        public async Task<IResult> UpdatePaymentStatusAsync(int id, string status)
        {
            try
            {
                _logger.LogInformation($"[MOCK PAYMENT] Ödeme durumu güncelleniyor: ID={id}");

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
                if (payment == null)
                {
                    _logger.LogWarning($"[MOCK PAYMENT] Ödeme bulunamadı: {id}");
                    return Result.Failure("Ödeme bulunamadı");
                }

                if (!Enum.TryParse<PaymentStatus>(status, out var paymentStatus))
                {
                    _logger.LogWarning($"[MOCK PAYMENT] Geçersiz durum: {status}");
                    return Result.Failure("Geçersiz ödeme durumu");
                }

                payment.Status = paymentStatus;
                payment.UpdatedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[MOCK PAYMENT] Ödeme durumu güncellendi: {paymentStatus}");
                return Result.Success("Ödeme durumu güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOCK PAYMENT] UpdatePaymentStatusAsync hatası");
                return Result.Failure("Ödeme durumu güncellenirken hata oluştu");
            }
        }

        public async Task<IResult> ProcessRefundAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation($"[MOCK PAYMENT] İade işleniyor: Payment={paymentId}");

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);
                if (payment == null)
                {
                    _logger.LogWarning($"[MOCK PAYMENT] Ödeme bulunamadı: {paymentId}");
                    return Result.Failure("Ödeme bulunamadı");
                }

                if (payment.Status != PaymentStatus.Completed)
                {
                    _logger.LogWarning($"[MOCK PAYMENT] İade yapılamaz");
                    return Result.Failure("Sadece tamamlanan ödemeler iade edilebilir");
                }

                payment.Status = PaymentStatus.Refunded;
                payment.UpdatedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[MOCK PAYMENT] İade işlendi");
                return Result.Success("İade başarıyla işlendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOCK PAYMENT] ProcessRefundAsync hatası");
                return Result.Failure("İade işlenirken hata oluştu");
            }
        }

        private static string SimulateBank(string bank, decimal amount)
        {
            return bank.ToLower() switch
            {
                "garanti" => amount < 50000 ? "Completed" : "Failed",
                "akbank" => amount < 100000 ? "Completed" : "Failed",
                "isbank" => amount < 75000 ? "Completed" : "Failed",
                _ => "Failed"
            };
        }
    }
}