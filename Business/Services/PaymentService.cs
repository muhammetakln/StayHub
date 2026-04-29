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
        // ✅ ADAPTÖRLER BURAYA ENJEKTE EDİLİYOR
        private readonly IEnumerable<IPaymentAdapter> _paymentAdapters;

        public PaymentService(
            StayHubContext context,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IEnumerable<IPaymentAdapter> paymentAdapters) // ✅ Constructor güncellendi
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _paymentAdapters = paymentAdapters;
        }

        // ✅ Ödeme Oluştur ve Bankadan Çek
        public async Task<IResult> CreatePaymentAsync(int reservationId, PaymentProcessDto dto)
        {
            try
            {
                _logger.LogInformation($"[PAYMENT] Ödeme işlemi başlatılıyor: Reservation={reservationId}");

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

                // 🌟 1. ADIM: STRATEJİ BELİRLE (Hangi banka / altyapı kullanılacak?)
                // Eğer formdan provider gelmiyorsa varsayılan olarak "Iyzico" kullanıyoruz
                string providerName = "Iyzico";
                var adapter = _paymentAdapters.FirstOrDefault(a => a.ProviderName == providerName);

                if (adapter == null)
                {
                    _logger.LogError($"[PAYMENT] Sistemde '{providerName}' isimli ödeme altyapısı bulunamadı!");
                    return Result.Failure("Ödeme altyapısı şu anda hizmet veremiyor.");
                }

                // 🌟 2. ADIM: BANKADAN PARAYI ÇEK
                var paymentResult = await adapter.ProcessPaymentAsync(dto);

                if (!paymentResult.IsSuccess)
                {
                    _logger.LogWarning($"[PAYMENT] Ödeme reddedildi. Hata: {paymentResult.ErrorMessage}");
                    return Result.Failure($"Ödeme işlemi banka tarafından reddedildi: {paymentResult.ErrorMessage}");
                }

                // 🌟 3. ADIM: BAŞARILI İŞLEMİ VERİTABANINA KAYDET
                var payment = new Payment
                {
                    ReservationId = reservationId,
                    PaymentReference = dto.OrderNumber ?? paymentResult.TransactionId,
                    Amount = dto.Amount,
                    PaymentMethod = providerName, // Hangi altyapıyla çekildiğini kaydediyoruz (Örn: Iyzico)
                    Status = PaymentStatus.Completed, // ✅ Para çekildiği için anında Completed yapıyoruz
                    TransactionId = paymentResult.TransactionId, // Bankadan dönen dekont ID'si
                    Notes = dto.Description,
                    PaymentDate = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);

                // Opsiyonel: Ödeme başarılı olduğu için Rezervasyon durumunu da "Onaylandı" yapalım
                reservation.Status = ReservationStatus.Confirmed;
                _context.Reservations.Update(reservation);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"[PAYMENT] Ödeme başarıyla çekildi ve kaydedildi: ID={payment.Id}");
                return Result.Success("Ödeme başarıyla tamamlandı!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENT] CreatePaymentAsync hatası");
                return Result.ServerError("Ödeme oluşturulurken sistemsel bir hata oluştu.");
            }
        }

        // ✅ Rezervasyona Göre Ödeme Al
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

        // ✅ Ödeme Durumu Güncelle
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

                if (!Enum.TryParse<PaymentStatus>(status, out var paymentStatus))
                {
                    _logger.LogWarning($"[PAYMENT] Geçersiz durum: {status}");
                    return Result.Failure("Geçersiz ödeme durumu");
                }

                payment.Status = paymentStatus;
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

        // ✅ İade İşle
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

                if (payment.Status != PaymentStatus.Completed && payment.Status != PaymentStatus.Pending)
                {
                    _logger.LogWarning($"[PAYMENT] İade yapılamaz");
                    return Result.Failure("Sadece tamamlanan ödemeler iade edilebilir");
                }

                // Not: Gerçek hayatta burada da Adapter çağrılır -> adapter.RefundPaymentAsync(payment.TransactionId)

                payment.Status = PaymentStatus.Refunded;
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