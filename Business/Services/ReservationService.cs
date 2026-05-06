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
using System.Globalization;

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
                // 1. Temel Kontroller
                var guest = await _context.Users.FindAsync(guestId) ?? throw new Exception("Misafir bulunamadı");
                var room = await _context.Rooms.Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoomId && !r.IsDeleted && r.IsActive)
                    ?? throw new Exception("Oda bulunamadı");

                if (dto.NumberOf > room.Capacity) throw new Exception($"Maksimum {room.Capacity} kişi kalabilir.");
                if (dto.CheckInDate >= dto.CheckOutDate) throw new Exception("Tarihler geçersiz.");

                // 2. Doluluk Kontrolü
                var isOccupied = await _context.Reservations.AnyAsync(r =>
                    r.RoomId == dto.RoomId && !r.IsDeleted && r.Status != ReservationStatus.Cancelled &&
                    r.CheckInDate < dto.CheckOutDate && r.CheckOutDate > dto.CheckInDate);
                if (isOccupied) throw new Exception("Oda bu tarihlerde dolu.");

                // 3. Fiyat ve Rezervasyon Kaydı
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

                // ✅ 4. EK HİZMETLER (HATA DÜZELTİLDİ)
                if (dto.SelectedServiceIds?.Any() == true)
                {
                    foreach (var sId in dto.SelectedServiceIds)
                    {
                        var service = await _context.AddOnServices.FindAsync(sId);
                        if (service == null) continue;

                        // Her iki tarafta decimal olduğu için doğrudan topluyoruz
                        reservation.TotalPrice += service.Price;

                        await _context.ReservationAddOnServices.AddAsync(new ReservationAddOnService
                        {
                            ReservationId = reservation.Id,
                            AddOnServiceId = sId,
                            Quantity = 1,
                            // ✅ HATA BURADAYDI: Entity sınıflarında her iki taraf da decimal olduğu için 
                            // doğrudan atama yapıyoruz. ToString() veya Parse'a gerek yok.
                            Price = service.Price,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                _ = SendInvoiceEmail(guest, reservation, room);

                return await GetReservationsByIdAsync(guestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rezervasyon Hatası");
                throw;
            }
        }

        private async Task SendInvoiceEmail(Guest guest, Reservation res, Room room)
        {
            try
            {
                var addons = await _context.ReservationAddOnServices
                    .Include(a => a.AddOnService)
                    .Where(a => a.ReservationId == res.Id)
                    .ToListAsync();

                string addonRows = string.Join("", addons.Select(a =>
                    $"<tr><td style='padding:5px;'>{a.AddOnService?.Name}</td><td align='right' style='padding:5px;'>{a.Price:N2} TL</td></tr>"));

                string paymentNote = "<div style='color:#e67e22; border:1px solid #e67e22; padding:10px; border-radius:5px; margin-top:20px;'>" +
                                     "<b>⚠️ ÖDEME BİLGİSİ:</b> Rezervasyonunuzun kesinleşmesi için ödemenizi giriş esnasında <b>resepsiyonda</b> yapabilirsiniz.</div>";

                string body = $@"
                <div style='font-family:sans-serif; border:1px solid #eee; padding:20px; max-width:550px; margin:auto;'>
                    <h2 style='color:#2c3e50; text-align:center;'>StayHub Rezervasyon Onay Belgesi</h2>
                    <hr>
                    <p>Sn. <b>{guest.FirstName} {guest.LastName}</b>,</p>
                    <table width='100%'>
                        <tr><td><b>Rezervasyon No:</b></td><td>{res.ReservationNumber}</td></tr>
                        <tr><td><b>Oda:</b></td><td>{room.RoomNumber}</td></tr>
                        <tr><td><b>Tarih:</b></td><td>{res.CheckInDate:dd.MM.yyyy} - {res.CheckOutDate:dd.MM.yyyy}</td></tr>
                        {addonRows}
                    </table>
                    <h3 style='color:#27ae60; text-align:right;'>Toplam: {res.TotalPrice:N2} TL</h3>
                    {paymentNote}
                </div>";

                await _emailSender.SendEmailAsync(guest.Email!, "StayHub Rezervasyon Onayı", body);
            }
            catch (Exception ex) { _logger.LogError(ex, "Fatura Mail Hatası"); }
        }

        public async Task<List<ReservationDto>> GetReservationsByIdAsync(int guestId) =>
            _mapper.Map<List<ReservationDto>>(await _context.Reservations.AsNoTracking().Include(r => r.Room)
                .Where(r => r.GuestId == guestId && !r.IsDeleted).OrderByDescending(r => r.CreatedAt).ToListAsync());

        public async Task<ReservationDto?> GetReservationByIdAsync(int id) =>
            _mapper.Map<ReservationDto>(await _context.Reservations.AsNoTracking().Include(r => r.Room).Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted));

        public async Task<IResult> CancelReservationAsync(int id)
        {
            var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (res == null || res.Status == ReservationStatus.Cancelled || DateTime.UtcNow > res.CheckInDate)
                return Result.Failure("İşlem yapılamaz.");

            res.Status = ReservationStatus.Cancelled;
            res.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result.Success("Rezervasyon iptal edildi.");
        }

        public async Task<IResult> UpdateReservationAsync(Reservation res)
        {
            _context.Reservations.Update(res);
            await _context.SaveChangesAsync();
            return Result.Success("Güncellendi.");
        }
    }
}