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
    public class ReservationService : IReservationService
    {
        private readonly StayHubContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(StayHubContext context, IMapper mapper, ILogger<ReservationService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        
        public async Task<List<ReservationDto>> CreateReservationAsync(int guestId, CreateReservationDto dto)
        {
            try
            {
                _logger.LogInformation($"[RESERVATION] Rezervasyon oluşturuluyor: Guest={guestId}, Room={dto.RoomId}");

                // Misafir var mı?
                var guestExists = await _context.Users.AnyAsync(g => g.Id == guestId);
                if (!guestExists)
                {
                    _logger.LogWarning($"[RESERVATION] Misafir bulunamadı: {guestId}");
                    throw new Exception("Misafir bulunamadı");
                }

               
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.Id == dto.RoomId && !r.IsDeleted && r.IsActive);

                if (room == null)
                {
                    _logger.LogWarning($"[RESERVATION] Oda bulunamadı: {dto.RoomId}");
                    throw new Exception("Oda bulunamadı");
                }

            
                if (dto.CheckInDate >= dto.CheckOutDate)
                {
                    _logger.LogWarning($"[RESERVATION] Geçersiz tarihler");
                    throw new Exception("Giriş tarihi çıkış tarihinden önce olmalıdır");
                }

               
                var conflictingReservation = await _context.Reservations
                    .AnyAsync(r =>
                        r.RoomId == dto.RoomId &&
                        !r.IsDeleted &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.CheckInDate < dto.CheckOutDate &&
                        r.CheckOutDate > dto.CheckInDate);

                if (conflictingReservation)
                {
                    _logger.LogWarning($"[RESERVATION] Oda bu tarihte müsait değil");
                    throw new Exception("Oda bu tarihte müsait değil");
                }

                
                var numberOfNights = (int)(dto.CheckOutDate - dto.CheckInDate).TotalDays;
                var totalPrice = numberOfNights * room.PricePerNight;

                var reservation = new Reservation
                {
                    ReservationNumber = GenerateReservationNumber(),
                    GuestId = guestId,
                    RoomId = dto.RoomId,
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
                  
                    NumberOfNights = numberOfNights,
                    PricePerNights = room.PricePerNight,
                    TotalPrice = totalPrice,
                    Status = ReservationStatus.Pending,
                    
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Reservations.AddAsync(reservation);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[RESERVATION] Rezervasyon oluşturuldu: ID={reservation.Id}, Ref={reservation.ReservationNumber}");

             
                return await GetReservationsByIdAsync(guestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RESERVATION] CreateReservationAsync hatası");
                throw;
            }
        }

     
        public async Task<List<ReservationDto>> GetReservationsByIdAsync(int guestId)
        {
            try
            {
                _logger.LogInformation($"[RESERVATION] Rezervasyonlar alınıyor: Guest={guestId}");

                var reservations = await _context.Reservations
                    .AsNoTracking()
                    .Where(r => r.GuestId == guestId && !r.IsDeleted)
                    .Include(r => r.Room)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"[RESERVATION] {reservations.Count} rezervasyon bulundu");
                return _mapper.Map<List<ReservationDto>>(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RESERVATION] GetReservationsByIdAsync hatası");
                return new List<ReservationDto>();
            }
        }

        
        public async Task<ReservationDto?> GetReservationByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"[RESERVATION] Rezervasyon alınıyor: ID={id}");

                var reservation = await _context.Reservations
                    .AsNoTracking()
                    .Include(r => r.Room)
                    .Include(r => r.Payments)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (reservation == null)
                {
                    _logger.LogWarning($"[RESERVATION] Rezervasyon bulunamadı: {id}");
                    return null;
                }

                return _mapper.Map<ReservationDto>(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RESERVATION] GetReservationByIdAsync hatası");
                return null;
            }
        }

       
        public async Task<IResult> CancelReservationAsync(int id)
        {
            try
            {
                _logger.LogInformation($"[RESERVATION] Rezervasyon iptal ediliyor: ID={id}");

                var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
                if (reservation == null)
                {
                    _logger.LogWarning($"[RESERVATION] Rezervasyon bulunamadı: {id}");
                    return Result.Failure("Rezervasyon bulunamadı");
                }

                if (reservation.Status == ReservationStatus.Cancelled)
                {
                    _logger.LogWarning($"[RESERVATION] Rezervasyon zaten iptal: {id}");
                    return Result.Failure("Rezervasyon zaten iptal edilmiştir");
                }

                if (DateTime.UtcNow > reservation.CheckInDate)
                {
                    _logger.LogWarning($"[RESERVATION] Check-in geçmiş, iptal edilemiyor: {id}");
                    return Result.Failure("Check-in zamanı geçtiği için iptal edilemiyor");
                }

                reservation.Status = ReservationStatus.Cancelled;
                reservation.CancelledAt = DateTime.UtcNow;
                reservation.UpdatedAt = DateTime.UtcNow;

                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[RESERVATION] Rezervasyon iptal edildi: ID={id}");
                return Result.Success("Rezervasyon başarıyla iptal edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RESERVATION] CancelReservationAsync hatası");
                return Result.Failure("Rezervasyon iptal edilirken hata oluştu");
            }
        }

        private string GenerateReservationNumber()
        {
            return $"RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}