using AutoMapper;
using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enum;
using Data.Contexts;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utils.Responses;

namespace Business.Services
{
    public class ReservationService : IReservationService
    {
        private readonly StayHubContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservationService> _logger;
        private readonly IEmailSender _emailSender;

        public ReservationService(StayHubContext context, IMapper mapper, ILogger<ReservationService> logger, IEmailSender emailSender)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<IResult<List<ReservationDto>>> CreateReservationAsync(int guestId, CreateReservationDto dto)
        {
            try
            {
                var guest = await _context.Users.OfType<Guest>().FirstOrDefaultAsync(u => u.Id == guestId);
                if (guest == null) return Result<List<ReservationDto>>.Failure("Misafir bulunamadı.");

                if (guest.DateOfBirth > DateTime.UtcNow.AddYears(-18))
                    return Result<List<ReservationDto>>.Failure("Yasal düzenlemeler gereği 18 yaşından küçükler rezervasyon yapamaz.");

                var room = await _context.Rooms.Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoomId && !r.IsDeleted && r.IsActive);

                if (room == null) return Result<List<ReservationDto>>.Failure("Oda bulunamadı veya şu an aktif değil.");

                if (dto.NumberOf > room.Capacity)
                    return Result<List<ReservationDto>>.Failure($"Maksimum {room.Capacity} kişi konaklayabilir.");

                if (dto.CheckInDate.Date < DateTime.UtcNow.Date)
                    return Result<List<ReservationDto>>.Failure("Geçmiş bir tarihe rezervasyon yapılamaz.");

                if (dto.CheckInDate.Date >= dto.CheckOutDate.Date)
                    return Result<List<ReservationDto>>.Failure("Çıkış tarihi giriş tarihinden sonra olmalıdır.");

                // 3. Çakışan Rezervasyon Kontrolü (Overlapping Check) - SQLite & Evrensel Uyumlu Versiyon
                var newCheckIn = dto.CheckInDate.Date;
                var newCheckOut = dto.CheckOutDate.Date;

                var isOccupied = await _context.Reservations.AsNoTracking().AnyAsync(r =>
                    r.RoomId == dto.RoomId && !r.IsDeleted && r.Status != ReservationStatus.Cancelled &&
                    (newCheckIn < r.CheckOutDate && newCheckOut > r.CheckInDate));

                if (isOccupied) return Result<List<ReservationDto>>.Failure("Seçilen oda bu tarihler arasında doludur.");

                // 4. Fiyat Hesaplama
                int nights = (int)(dto.CheckOutDate.Date - dto.CheckInDate.Date).TotalDays;
                decimal extraPrice = dto.NumberOf > 1 ? (dto.NumberOf - 1) * 150 : 0;
                decimal totalPrice = (room.Price * nights) + extraPrice;

                var reservation = new Reservation
                {
                    ReservationNumber = $"RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    GuestId = guestId,
                    RoomId = dto.RoomId,
                    CheckInDate = dto.CheckInDate.Date,
                    CheckOutDate = dto.CheckOutDate.Date,
                    NumberOf = dto.NumberOf,
                    NumberOfNights = nights,
                    PricePerNights = room.Price,
                    TotalPrice = totalPrice,
                    Status = ReservationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Reservations.AddAsync(reservation);
                await _context.SaveChangesAsync();

                if (dto.SelectedServiceIds?.Any() == true)
                {
                    foreach (var sId in dto.SelectedServiceIds)
                    {
                        var service = await _context.AddOnServices.FindAsync(sId);
                        if (service == null) continue;

                        reservation.TotalPrice += service.Price;
                        await _context.ReservationAddOnServices.AddAsync(new ReservationAddOnService
                        {
                            ReservationId = reservation.Id,
                            AddOnServiceId = sId,
                            Quantity = 1,
                            Price = service.Price,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                // 🎯 DÜZENLENDİ: Ödeme yapılmadan fatura maili gitmemesi için buradaki SendInvoiceEmail çağrısı kaldırıldı.
                // Fatura maili, ödeme servisinde (PaymentService vb.) ödeme başarılı (Success) tetiklendiğinde çağrılmalıdır.

                var reservations = await GetReservationsByIdAsync(guestId);
                return Result<List<ReservationDto>>.Success(reservations, "Rezervasyonunuz başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear(); // Hata durumunda context state'ini temizle
                _logger.LogError(ex, "CreateReservationAsync Hatası");
                return Result<List<ReservationDto>>.Failure("İşlem sırasında teknik bir hata oluştu.");
            }
        }


        public async Task<List<ReservationDto>> GetReservationsByIdAsync(int guestId)
        {
            var data = await _context.Reservations
                .Include(r => r.Room).ThenInclude(rm => rm.Hotel)
                .Where(r => r.GuestId == guestId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtoList = _mapper.Map<List<ReservationDto>>(data);

            for (int i = 0; i < data.Count; i++)
            {
                dtoList[i].HotelName = data[i].Room?.Hotel?.Name ?? "StayHub Otel";
                dtoList[i].RoomNumber = data[i].Room?.RoomNumber ?? "N/A";
            }

            return dtoList;
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(int id)
        {
            var data = await _context.Reservations
                .Include(r => r.Room).ThenInclude(rm => rm.Hotel)
                .Include(r => r.SelectedServices).ThenInclude(s => s.AddOnService)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (data == null) return null;

            var dto = _mapper.Map<ReservationDto>(data);
            dto.HotelName = data.Room?.Hotel?.Name ?? "StayHub Otel";
            dto.RoomNumber = data.Room?.RoomNumber ?? "N/A";
            return dto;
        }


        public async Task<IResult> UpdateReservationAsync(Reservation reservation)
        {
            try
            {
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();
                return Result.Success("Rezervasyon güncellendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateReservationAsync Hatası");
                return Result.Failure("Güncelleme başarısız.");
            }
        }


        public async Task<IResult> CancelReservationAsync(int id)
        {
            try
            {
                var res = await _context.Reservations
                    .Include(r => r.Payments)
                    .Include(r => r.Guest)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (res == null) return Result.Failure("Rezervasyon bulunamadı.");
                if (res.Status == ReservationStatus.Cancelled) return Result.Failure("Bu rezervasyon zaten iptal edilmiş.");

                var timeUntilCheckIn = res.CheckInDate - DateTime.UtcNow;
                bool isRefundable = timeUntilCheckIn.TotalHours >= 24;
                string refundMessage = "";

                if (res.Payments != null && res.Payments.Any())
                {
                    if (isRefundable)
                    {
                        foreach (var payment in res.Payments)
                        {
                            if (payment.Status == PaymentStatus.Success)
                            {
                                payment.Status = PaymentStatus.Refunded;
                                payment.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                        refundMessage = " Ödemeleriniz iade edilmek üzere işleme alınmıştır.";
                        _logger.LogInformation($"Rezervasyon {res.ReservationNumber} için iade onaylandı.");
                    }
                    else
                    {
                        refundMessage = " Giriş tarihinize 24 saatten az kaldığı için iade yapılamamaktadır.";
                        _logger.LogWarning($"Rezervasyon {res.ReservationNumber} geç iptal, iade reddedildi.");
                    }
                }

                // Rezervasyon durumunu güncelle
                res.Status = ReservationStatus.Cancelled;
                res.CancelledAt = DateTime.UtcNow;

                _context.Reservations.Update(res);
                await _context.SaveChangesAsync();

                // 🎯 DÜZENLENDİ: İptal maili tetikleyicisi eklendi
                await SendCancellationEmail(res.Id);

                return Result.Success($"Rezervasyon başarıyla iptal edildi.{refundMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancelReservationAsync Hatası");
                return Result.Failure("İptal işlemi sırasında bir hata oluştu.");
            }
        }



        public async Task SendInvoiceEmail(Guest guest, Reservation reservation, Room room)
        {
            try
            {
                string guestName = $"{guest.FirstName} {guest.LastName}";
                string body = $@"
                <div style='font-family:Arial; border:1px solid #2ecc71; padding:20px; max-width: 600px; margin: auto;'>
                    <h2 style='color:#27ae60; text-align: center;'>🧾 Rezervasyon Onayı ve Fatura</h2>
                    <p>Sayın <b>{guestName}</b>, rezervasyonunuz onaylanmıştır.</p>
                    <div style='background-color:#f9f9f9; padding: 15px; border-radius: 8px;'>
                        <p><b>🔑 Rezervasyon No:</b> {reservation.ReservationNumber}</p>
                        <p><b>🏨 Otel:</b> {room.Hotel?.Name ?? "StayHub"}</p>
                        <p><b>📅 Tarih:</b> {reservation.CheckInDate:dd.MM.yyyy} - {reservation.CheckOutDate:dd.MM.yyyy}</p>
                        <h3 style='color: #c0392b; text-align: right;'>Toplam: {reservation.TotalPrice:N2} TL</h3>
                    </div>
                </div>";

                await _emailSender.SendEmailAsync(guest.Email!, $"StayHub Fatura - {reservation.ReservationNumber}", body);
            }
            catch (Exception ex) { _logger.LogError(ex, "Fatura mail hatası"); }
        }

        public async Task SendCancellationEmail(int reservationId)
        {
            try
            {
                var res = await _context.Reservations.Include(r => r.Guest).FirstOrDefaultAsync(r => r.Id == reservationId);
                if (res?.Guest != null)
                {
                    string body = $"<h2>İptal Onayı</h2><p>Sn. {res.Guest.FirstName}, {res.ReservationNumber} nolu rezervasyonunuz iptal edilmiştir.</p>";
                    await _emailSender.SendEmailAsync(res.Guest.Email!, "Rezervasyon İptal Bildirimi", body);
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "İptal mail hatası"); }
        }

        public async Task<decimal> GetMonthlyRevenueByHotelIdAsync(int hotelId)
        {
            var monthAgo = DateTime.UtcNow.AddDays(-30);
            return await _context.Reservations
                .Where(r => r.Room.HotelId == hotelId && r.Status == ReservationStatus.Confirmed && !r.IsDeleted && r.CreatedAt >= monthAgo)
                .SumAsync(r => r.TotalPrice);
        }
    }
}