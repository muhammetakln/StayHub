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

        // Mock Payment Database (In-Memory)
        private static readonly Dictionary<string, PaymentStatus> MockDatabase = new();

        public MockPaymentService(StayHubContext context, ILogger<MockPaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ Ödeme Oluştur
        public async Task<IResult> CreatePaymentAsync(int reservationId, PaymentProcessDto dto)
        {
            try
            {
                _logger.LogInformation($"[MOCK PAYMENT] Ödeme oluşturuluyor: Reservation={reservationId}");

                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                {
                    _logger.LogWarning($"[MOCK PAYMENT] Rezervasyon bulunamadı: {reservationId}");
                    return Result.Failure("Rezervasyon bulunamadı");
                }

                // ✅ Card validation
                if (string.IsNullOrEmpty(dto.CardNumber) || dto.CardNumber.Length < 13)
                {
                    _logger.LogWarning($"[MOCK PAYMENT] Geçersiz kart numarası");
                    return Result.Failure("Geçersiz kart numarası");
                }

                // ✅ Mock bank simulation
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

                return Result.Success($"Ödeme {(bankStatus == "Completed" ? "başarılı" : "başarısız")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MOCK PAYMENT] CreatePaymentAsync hatası");
                return Result.Failure("Ödeme oluşturulurken hata oluştu");
            }
        }

        // ✅ Rezervasyona Göre Ödeme Al
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

        // ✅ Ödeme Durumu Güncelle
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

        // ✅ İade İşle
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

        // ✅ MOCK BANK SIMULATION
        private static string SimulateBank(string bank, decimal amount)
        {
            return bank.ToLower() switch
            {
                "garanti" => amount < 50000 ? "Completed" : "Failed",      // Garanti: < 50K TRY
                "akbank" => amount < 100000 ? "Completed" : "Failed",      // Akbank: < 100K TRY
                "isbank" => amount < 75000 ? "Completed" : "Failed",       // İş Bank: < 75K TRY
                _ => "Failed"
            };
        }
    }
}