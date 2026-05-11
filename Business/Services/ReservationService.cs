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

                // Temel oda fiyatı hesaplama
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
                    TotalPrice = totalPrice, // İlk başta sadece oda fiyatı
                    Status = ReservationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Reservations.AddAsync(reservation);
                await _context.SaveChangesAsync(); // ID oluşması için kaydediyoruz

                // ✅ EK HİZMETLERİ İŞLEME VE TOPLAM FİYATA EKLEME
                if (dto.SelectedServiceIds != null && dto.SelectedServiceIds.Any())
                {
                    foreach (var sId in dto.SelectedServiceIds)
                    {
                        var service = await _context.AddOnServices
                            .FirstOrDefaultAsync(s => s.Id == sId && !s.IsDeleted && s.IsActive);

                        if (service == null) continue;

                        // Toplam fiyata hizmet bedelini ekle
                        reservation.TotalPrice += service.Price;

                        // Ara tabloya (ReservationAddOnService) kaydet
                        await _context.ReservationAddOnServices.AddAsync(new ReservationAddOnService
                        {
                            ReservationId = reservation.Id,
                            AddOnServiceId = sId,
                            Quantity = 1,
                            Price = service.Price, // O anki fiyatı sabitliyoruz
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Toplam fiyat ve hizmetler için son bir save
                    await _context.SaveChangesAsync();
                }

                if (room.Hotel != null && !string.IsNullOrEmpty(room.Hotel.Email))
                {
                    await SendNotificationToHotel(room.Hotel, reservation, room, guest);
                }

                return await GetReservationsByIdAsync(guestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyon Hatası");
                throw;
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
                    <p>Sisteminiz üzerinden yeni bir rezervasyon oluşturulmuştur. Lütfen ilgili odayı misafiriniz için hazırlayınız. Detaylar aşağıdadır:</p>
                    
                    <div style='background-color:#f9f9f9; padding: 15px; border-radius: 8px; margin-bottom: 20px;'>
                        <p style='margin: 5px 0;'><b>🔑 Rezervasyon No:</b> {res.ReservationNumber}</p>
                        <p style='margin: 5px 0;'><b>👤 Misafir Adı:</b> {guestName}</p>
                        <p style='margin: 5px 0;'><b>🚪 Oda:</b> {room.RoomNumber} ({room.Name})</p>
                        <p style='margin: 5px 0;'><b>📅 Giriş Tarihi:</b> {res.CheckInDate:dd.MM.yyyy}</p>
                        <p style='margin: 5px 0;'><b>📅 Çıkış Tarihi:</b> {res.CheckOutDate:dd.MM.yyyy}</p>
                        <p style='margin: 5px 0;'><b>👥 Kişi Sayısı:</b> {res.NumberOf}</p>
                        <h3 style='margin: 15px 0 0 0; color: #e74c3c;'>Toplam Tutar: {res.TotalPrice:N2} TL</h3>
                    </div>
                    
                    <p style='font-size:12px; color:gray; text-align: center; border-top: 1px solid #ddd; padding-top: 10px;'>
                        Bu mesaj StayHub Otomasyon Sistemi tarafından otomatik olarak gönderilmiştir. Lütfen bu maile cevap vermeyiniz.
                    </p>
                </div>";

                await _emailSender.SendEmailAsync(hotel.Email!, $"Yeni Rezervasyon: {res.ReservationNumber} - Oda {room.RoomNumber}", body);
                _logger.LogInformation($"Otele bildirim maili gönderildi: {hotel.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel bildirim maili gönderilirken hata oluştu.");
            }
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
                .Select(a => _mapper.Map<AddOnServiceDto>(a))
                .ToList();

            return dto;
        }

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

                string name = reservation.Guest is Guest g ? g.FirstName : reservation.Guest.UserName;

                string body = $"<h2>İptal Bildirimi</h2><p>Sn. {name}, {reservation.ReservationNumber} nolu kaydınız iptal edildi.</p>";
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

        }
    }
}