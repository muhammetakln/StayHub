using AutoMapper;
using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enum;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utils.Responses;

namespace Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StayHubContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(StayHubContext context, IMapper mapper, ILogger<PaymentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

      
        public async Task<IResult> CreatePaymentAsync(int reservationId, PaymentProcessDto dto)
        {
            try
            {
                _logger.LogInformation($"[PAYMENT] Ödeme oluşturuluyor: Reservation={reservationId}");

                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                {
                    _logger.LogWarning($"[PAYMENT] Rezervasyon bulunamadı: {reservationId}");
                    return Result.Failure("Rezervasyon bulunamadı");
                }

                if (string.IsNullOrEmpty(dto.CardNumber) || dto.CardNumber.Length < 13)
                {
                    _logger.LogWarning($"[PAYMENT] Geçersiz kart numarası");
                    return Result.Failure("Geçersiz kart numarası");
                }

                var payment = new Payment
                {
                    ReservationId = reservationId,
                    PaymentReference = dto.OrderNumber,
                    Amount = dto.Amount,
                    PaymentMethod = "Credit Card",
                    Status = RoomStatus.Pending,
                    TransactionId = Guid.NewGuid().ToString(),
                    Notes = dto.Description,
                    PaymentDate = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[PAYMENT] Ödeme oluşturuldu: ID={payment.Id}");
                return Result.Success("Ödeme başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENT] CreatePaymentAsync hatası");
                return Result.Failure("Ödeme oluşturulurken hata oluştu");
            }
        }

      
        public async Task<PaymentDetailDto?> GetPaymentByReservationIdAsync(int reservationId)
        {
            try
            {
                _logger.LogInformation($"[PAYMENT] Ödeme alınıyor: Reservation={reservationId}");

                var payment = await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ReservationId == reservationId && !p.IsDeleted);

                if (payment == null)
                {
                    _logger.LogWarning($"[PAYMENT] Ödeme bulunamadı: {reservationId}");
                    return null;
                }

                return _mapper.Map<PaymentDetailDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENT] GetPaymentByReservationIdAsync hatası");
                return null;
            }
        }

      
        public async Task<IResult> UpdatePaymentStatusAsync(int id, string status)
        {
            try
            {
                _logger.LogInformation($"[PAYMENT] Ödeme durumu güncelleniyor: ID={id}");

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
                if (payment == null)
                {
                    _logger.LogWarning($"[PAYMENT] Ödeme bulunamadı: {id}");
                    return Result.Failure("Ödeme bulunamadı");
                }

                
                if (!Enum.TryParse<RoomStatus>(status, out var roomStatus))
                {
                    _logger.LogWarning($"[PAYMENT] Geçersiz durum: {status}");
                    return Result.Failure("Geçersiz ödeme durumu");
                }

                payment.Status = roomStatus; 
                payment.UpdatedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[PAYMENT] Ödeme durumu güncellendi");
                return Result.Success("Ödeme durumu güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENT] UpdatePaymentStatusAsync hatası");
                return Result.Failure("Ödeme durumu güncellenirken hata oluştu");
            }
        }

     
        public async Task<IResult> ProcessRefundAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation($"[PAYMENT] İade işleniyor: Payment={paymentId}");

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);
                if (payment == null)
                {
                    _logger.LogWarning($"[PAYMENT] Ödeme bulunamadı: {paymentId}");
                    return Result.Failure("Ödeme bulunamadı");
                }

                if (payment.Status != RoomStatus.Confirmed && payment.Status != RoomStatus.Pending)
                {
                    _logger.LogWarning($"[PAYMENT] İade yapılamaz");
                    return Result.Failure("Sadece tamamlanan ödemeler iade edilebilir");
                }

                payment.Status = RoomStatus.Cancelled; 
                payment.UpdatedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[PAYMENT] İade işlendi");
                return Result.Success("İade başarıyla işlendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENT] ProcessRefundAsync hatası");
                return Result.Failure("İade işlenirken hata oluştu");
            }
        }
    }
}