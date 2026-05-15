using AutoMapper;
using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enum;
using Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utils.Responses;
using System.Linq;

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

        // ✅ Geri dönüş tipi arayüzle uyumlu hale getirildi: Task<IResult<List<ReservationDto>>>
        public async Task<IResult<List<ReservationDto>>> CreateReservationAsync(int guestId, CreateReservationDto dto)
        {
            try
            {
                var guest = await _context.Users.FindAsync(guestId);
                if (guest == null) return Result<List<ReservationDto>>.Failure("Misafir bulunamadı.");

                var room = await _context.Rooms.Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoomId && !r.IsDeleted && r.IsActive);

                if (room == null) return Result<List<ReservationDto>>.Failure("Oda bulunamadı.");

                if (dto.NumberOf > room.Capacity)
                    return Result<List<ReservationDto>>.Failure($"Maksimum {room.Capacity} kişi kalabilir.");

                if (dto.CheckInDate < DateTime.UtcNow.Date)
                    return Result<List<ReservationDto>>.Failure("Geçmiş bir tarihe rezervasyon yapılamaz.");

                if (dto.CheckInDate >= dto.CheckOutDate)
                    return Result<List<ReservationDto>>.Failure("Tarihler geçersiz.");

                var isOccupied = await _context.Reservations.AsNoTracking().AnyAsync(r =>
                    r.RoomId == dto.RoomId && !r.IsDeleted && r.Status != ReservationStatus.Cancelled &&
                    r.CheckInDate < dto.CheckOutDate && r.CheckOutDate > dto.CheckInDate);

                if (isOccupied) return Result<List<ReservationDto>>.Failure("Oda bu tarihlerde dolu.");

                int nights = (int)(dto.CheckOutDate - dto.CheckInDate).TotalDays;
                decimal totalPrice = (room.Price * nights) + (dto.NumberOf > 1 ? (dto.NumberOf - 1) * 150 : 0);

                var reservation = new Reservation
                {
                    ReservationNumber = $"RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    GuestId = guestId,
                    RoomId = dto.RoomId,
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
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

                if (room.Hotel != null && !string.IsNullOrEmpty(room.Hotel.Email))
                {
                    _ = SendNotificationToHotel(room.Hotel, reservation, room, guest);
                }

                var reservations = await GetReservationsByIdAsync(guestId);
                return Result<List<ReservationDto>>.Success(reservations, "Rezervasyon başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyon Hatası");
                return Result<List<ReservationDto>>.Failure("İşlem sırasında bir hata oluştu.");
            }
        }

        public async Task SendNotificationToHotel(Hotel hotel, Reservation res, Room room, IdentityUser<int> guest)
        {
            try
            {
                string guestName = guest is Guest g ? $"{g.FirstName} {g.LastName}" : guest.UserName ?? "Misafir";
                string body = $@"
                <div style='font-family:Arial; border:1px solid #3498db; padding:20px; max-width: 600px;'>
                    <h2 style='color:#2c3e50;'>🔔 Yeni Rezervasyon Bildirimi</h2>
                    <p>Sayın <b>{hotel.Name}</b> Yetkilisi,</p>
                    <div style='background-color:#f9f9f9; padding: 15px; border-radius: 8px;'>
                        <p><b>🔑 Rezervasyon No:</b> {res.ReservationNumber}</p>
                        <p><b>👤 Misafir:</b> {guestName}</p>
                        <p><b>🚪 Oda:</b> {room.RoomNumber}</p>
                        <p><b>📅 Tarih:</b> {res.CheckInDate:dd.MM.yyyy} - {res.CheckOutDate:dd.MM.yyyy}</p>
                        <h3 style='color: #e74c3c;'>Tutar: {res.TotalPrice:N2} TL</h3>
                    </div>
                </div>";
                await _emailSender.SendEmailAsync(hotel.Email!, $"Yeni Rezervasyon: {res.ReservationNumber}", body);
            }
            catch (Exception ex) { _logger.LogError(ex, "Otel mail hatası"); }
        }

        public async Task<List<ReservationDto>> GetReservationsByIdAsync(int guestId)
        {
            var reservationsRaw = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.GuestId == guestId && !r.IsDeleted)
                .Select(r => new {
                    Reservation = r,
                    RoomData = r.Room,
                    HotelData = r.Room.Hotel
                })
                .OrderByDescending(x => x.Reservation.CreatedAt)
                .ToListAsync();

            var dtoList = new List<ReservationDto>();
            foreach (var item in reservationsRaw)
            {
                var dto = _mapper.Map<ReservationDto>(item.Reservation);
                dto.HotelName = item.HotelData?.Name ?? "StayHub Otel";
                dto.HotelId = item.HotelData?.Id ?? 0;
                dto.RoomNumber = item.RoomData?.RoomNumber ?? "N/A";
                dto.RoomName = item.RoomData?.Name;
                dtoList.Add(dto);
            }
            return dtoList;
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(int id)
        {
            var item = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Id == id && !r.IsDeleted)
                .Select(r => new {
                    Res = r,
                    RoomData = r.Room,
                    HotelData = r.Room.Hotel,
                    AddOns = r.SelectedServices.Select(ras => ras.AddOnService)
                })
                .FirstOrDefaultAsync();

            if (item == null) return null;

            var dto = _mapper.Map<ReservationDto>(item.Res);
            dto.HotelName = item.HotelData?.Name ?? "StayHub Otel";
            dto.HotelId = item.HotelData?.Id ?? 0;
            dto.RoomName = item.RoomData?.Name;
            dto.RoomNumber = item.RoomData?.RoomNumber ?? "N/A";

            dto.SelectedServices = item.AddOns
                .Where(a => a != null)
                .Select(a => _mapper.Map<AddOnServiceDto>(a))
                .ToList();

            return dto;
        }

        public async Task<IResult> CancelReservationAsync(int id)
        {
            try
            {
                var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
                if (res == null) return Result.Failure("Rezervasyon bulunamadı.");
                if (res.Status == ReservationStatus.Cancelled) return Result.Failure("Zaten iptal edilmiş.");

                res.Status = ReservationStatus.Cancelled;
                res.CancelledAt = DateTime.UtcNow;

                _context.Reservations.Update(res);
                await _context.SaveChangesAsync();

                return Result.Success("Rezervasyon başarıyla iptal edildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İptal Hatası");
                return Result.Failure("İptal işlemi başarısız.");
            }
        }

        public async Task SendCancellationEmail(int reservationId)
        {
            try
            {
                var reservation = await _context.Reservations.Include(r => r.Guest).FirstOrDefaultAsync(r => r.Id == reservationId);
                if (reservation == null || reservation.Guest == null) return;
                string name = reservation.Guest.FirstName;
                string body = $"<h2>İptal Bildirimi</h2><p>Sn. {name}, {reservation.ReservationNumber} nolu kaydınız iptal edildi.</p>";
                await _emailSender.SendEmailAsync(reservation.Guest.Email!, "Rezervasyon İptali", body);
            }
            catch (Exception ex) { _logger.LogError(ex, "Mail Hatası"); }
        }

        public async Task<IResult> UpdateReservationAsync(Reservation res)
        {
            try
            {
                _context.Reservations.Update(res);
                await _context.SaveChangesAsync();
                return Result.Success("Rezervasyon güncellendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Güncelleme Hatası");
                return Result.Failure("Güncelleme başarısız.");
            }
        }

        // ✅ HTML Fatura Tasarımı Buraya Eklendi
        public async Task SendInvoiceEmail(Guest guest, Reservation res, Room room)
        {
            try
            {
                string guestName = !string.IsNullOrEmpty(guest.FirstName) ? $"{guest.FirstName} {guest.LastName}" : guest.UserName ?? "Misafir";
                string body = $@"
                <div style='font-family:Arial; border:1px solid #2ecc71; padding:20px; max-width: 600px; margin: auto;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h2 style='color:#27ae60; margin: 0;'>🧾 Rezervasyon Özeti ve Fatura</h2>
                        <p style='color: #7f8c8d;'>StayHub Konaklama Onayı</p>
                    </div>
                    <p>Sayın <b>{guestName}</b>,</p>
                    <p>Bizi tercih ettiğiniz için teşekkür ederiz. Rezervasyon işleminiz tamamlanmıştır.</p>
                    <div style='background-color:#f9f9f9; padding: 15px; border-radius: 8px; border-left: 5px solid #2ecc71; margin: 20px 0;'>
                        <p style='margin: 5px 0;'><b>🔑 Rezervasyon No:</b> {res.ReservationNumber}</p>
                        <p style='margin: 5px 0;'><b>🏨 Otel:</b> {room.Hotel?.Name ?? "StayHub Otel"}</p>
                        <p style='margin: 5px 0;'><b>🚪 Oda:</b> {room.RoomNumber} ({room.Name})</p>
                        <p style='margin: 5px 0;'><b>📅 Tarih:</b> {res.CheckInDate:dd.MM.yyyy} - {res.CheckOutDate:dd.MM.yyyy}</p>
                        <hr style='border: 0; border-top: 1px solid #eee;'>
                        <h3 style='margin: 15px 0 0 0; color: #c0392b; text-align: right;'>Toplam Tutar: {res.TotalPrice:N2} TL</h3>
                    </div>
                    <div style='text-align: center; color: #95a5a6; font-size: 12px; margin-top: 30px;'>
                        <p>İyi konaklamalar dileriz!<br><b>StayHub Ekibi</b></p>
                    </div>
                </div>";

                await _emailSender.SendEmailAsync(guest.Email!, $"Rezervasyon Onayı - {res.ReservationNumber}", body);
                _logger.LogInformation($"[EMAIL] Fatura gönderildi: {guest.Email}");
            }
            catch (Exception ex) { _logger.LogError(ex, "Fatura mail hatası"); }
        }

        public async Task<decimal> GetMonthlyRevenueByHotelIdAsync(int hotelId)
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                return await _context.Reservations
                    .AsNoTracking()
                    .Where(r => r.Room.HotelId == hotelId && r.Status == ReservationStatus.Confirmed && !r.IsDeleted && r.CreatedAt >= thirtyDaysAgo)
                    .SumAsync(r => r.TotalPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelir hesaplama hatası");
                return 0;
            }
        }
    }
}