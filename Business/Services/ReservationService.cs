using AutoMapper;
using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enum;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utils.Responses;
using Microsoft.AspNetCore.Identity.UI.Services;

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

        // ✅ REZERVASYON OLUŞTURMA
        public async Task<List<ReservationDto>> CreateReservationAsync(int guestId, CreateReservationDto dto)
        {
            try
            {
                var guest = await _context.Users.FindAsync(guestId) ?? throw new Exception("Misafir bulunamadı");
                var room = await _context.Rooms.Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoomId && !r.IsDeleted && r.IsActive)
                    ?? throw new Exception("Oda bulunamadı");

                if (dto.NumberOf > room.Capacity) throw new Exception($"Maksimum {room.Capacity} kişi kalabilir.");
                if (dto.CheckInDate >= dto.CheckOutDate) throw new Exception("Tarihler geçersiz.");

                var isOccupied = await _context.Reservations.AsNoTracking().AnyAsync(r =>
                    r.RoomId == dto.RoomId && !r.IsDeleted && r.Status != ReservationStatus.Cancelled &&
                    r.CheckInDate < dto.CheckOutDate && r.CheckOutDate > dto.CheckInDate);

                if (isOccupied) throw new Exception("Oda bu tarihlerde dolu.");

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
                    // 🛡️ HotelId atanmadı, veritabanında yok.
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

                return await GetReservationsByIdAsync(guestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyon Hatası");
                throw;
            }
        }

        // ✅ REZERVASYONLARI LİSTELEME (MİSAFİR İÇİN)
        public async Task<List<ReservationDto>> GetReservationsByIdAsync(int guestId)
        {
            // 🛡️ SQLite'ın r.HotelId aramasını engellemek için Projection (Select) kullanıyoruz.
            // Bu yöntem veritabanından sadece var olan sütunları çeker.
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

                // 🛡️ DTO'daki HotelId ve isim alanlarını elle dolduruyoruz.
                dto.HotelName = item.HotelData?.Name ?? "StayHub Otel";
                dto.HotelId = item.HotelData?.Id ?? 0;
                dto.RoomNumber = item.RoomData?.RoomNumber ?? "N/A";
                dto.RoomName = item.RoomData?.Name;

                dtoList.Add(dto);
            }

            return dtoList;
        }

        // ✅ TEKİL REZERVASYON DETAYI
        public async Task<ReservationDto?> GetReservationByIdAsync(int id)
        {
            // Yine aynı mantıkla r.HotelId kolonuna basmaması için Select kullanıyoruz.
            var item = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Id == id && !r.IsDeleted)
                .Select(r => new {
                    Res = r,
                    RoomData = r.Room,
                    HotelData = r.Room.Hotel,
                    AddOns = r.AddOnServices.Select(ras => ras.AddOnService)
                })
                .FirstOrDefaultAsync();

            if (item == null) return null;

            var dto = _mapper.Map<ReservationDto>(item.Res);

            dto.HotelName = item.HotelData?.Name ?? "StayHub Otel";
            dto.HotelId = item.HotelData?.Id ?? 0;
            dto.RoomName = item.RoomData?.Name;
            dto.RoomNumber = item.RoomData?.RoomNumber ?? "N/A";

            dto.SelectedServices = item.AddOns
                .Select(a => _mapper.Map<AddOnServiceDto>(a))
                .ToList();

            return dto;
        }

        // --- DİĞER YARDIMCI METOTLAR ---

        public async Task<IResult> CancelReservationAsync(int id)
        {
            var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (res == null) return Result.Failure("Rezervasyon bulunamadı.");
            if (res.Status == ReservationStatus.Cancelled) return Result.Failure("Zaten iptal edilmiş.");

            res.Status = ReservationStatus.Cancelled;
            res.CancelledAt = DateTime.UtcNow;
            _context.Reservations.Update(res);
            await _context.SaveChangesAsync();
            return Result.Success("İptal edildi.");
        }

        public async Task SendCancellationEmail(int reservationId)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Guest)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);
                if (reservation == null || reservation.Guest == null) return;

                string body = $"<h2>İptal Bildirimi</h2><p>Sn. {reservation.Guest.FirstName}, {reservation.ReservationNumber} nolu kaydınız iptal edildi.</p>";
                await _emailSender.SendEmailAsync(reservation.Guest.Email!, "Rezervasyon İptali", body);
            }
            catch (Exception ex) { _logger.LogError(ex, "Mail Hatası"); }
        }

        public async Task<IResult> UpdateReservationAsync(Reservation res)
        {
            _context.Reservations.Update(res);
            await _context.SaveChangesAsync();
            return Result.Success("Güncellendi.");
        }

        public async Task SendInvoiceEmail(Guest guest, Reservation res, Room room)
        {
            // ... (Invoice mail mantığın aynı kalabilir)
        }
    }
}