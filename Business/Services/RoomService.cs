using AutoMapper;
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
    public class RoomService : IRoomService
    {
        private readonly StayHubContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RoomService> _logger;

        public RoomService(StayHubContext context, IMapper mapper, ILogger<RoomService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ✅ Dönüş tipi IResult<RoomDto> olarak güncellendi
        public async Task<IResult<RoomDto>> CreateRoomByIdAsync(int hotelId, CreateRoomDto dto)
        {
            try
            {
                _logger.LogInformation($"[ROOM] Oda oluşturuluyor: Hotel={hotelId}, Name={dto.Name}");

                var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == hotelId && !h.IsDeleted);
                if (!hotelExists)
                {
                    _logger.LogWarning($"[ROOM] Otel bulunamadı: {hotelId}");
                    return Result<RoomDto>.Failure("Otel bulunamadı");
                }

                var roomNumber = $"ROOM-{hotelId}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";

                var room = new Room
                {
                    HotelId = hotelId,
                    RoomNumber = roomNumber,
                    Name = dto.Name,
                    Description = dto.Description,
                    Capacity = dto.Capacity,
                    Size = dto.Size,
                    Price = dto.Price,
                    PricePerNight = dto.Price,
                    Status = RoomStatus.Available,
                    IsActive = dto.IsActive,
                    FloorNumber = 1,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Rooms.AddAsync(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[ROOM] Oda oluşturuldu: ID={room.Id}, RoomNumber={room.RoomNumber}");

                // ✅ Oluşturulan oda bilgisini DTO'ya çevirip geri döndürüyoruz (Data taşımak için)
                var roomDto = new RoomDto
                {
                    Id = room.Id,
                    Name = room.Name,
                    RoomNumber = room.RoomNumber,
                    Price = room.Price,
                    IsActive = room.IsActive
                };

                return Result<RoomDto>.Success(roomDto, "Oda başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ROOM] CreateRoomByIdAsync hatası");
                return Result<RoomDto>.Failure("Oda oluşturulurken hata oluştu");
            }
        }

        public async Task<IResult> UpdateRoomAsync(int roomId, UpdateRoomDto dto)
        {
            try
            {
                _logger.LogInformation($"[ROOM] Oda güncelleniyor: ID={roomId}");

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);
                if (room == null)
                {
                    _logger.LogWarning($"[ROOM] Oda bulunamadı: {roomId}");
                    return Result.Failure("Oda bulunamadı");
                }

                room.Name = dto.Name;
                room.Description = dto.Description;
                room.Capacity = dto.Capacity;
                room.Size = dto.Size;
                room.Price = dto.Price;
                room.PricePerNight = dto.Price;
                room.IsActive = dto.IsActive;
                room.UpdatedAt = DateTime.UtcNow;

                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[ROOM] Oda güncellendi: ID={roomId}");
                return Result.Success("Oda başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ROOM] UpdateRoomAsync hatası");
                return Result.Failure("Oda güncellenirken hata oluştu");
            }
        }

        public async Task<IResult> DeleteRoomAsync(int roomId)
        {
            try
            {
                _logger.LogInformation($"[ROOM] Oda siliniyor: ID={roomId}");

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);
                if (room == null)
                {
                    _logger.LogWarning($"[ROOM] Oda bulunamadı: {roomId}");
                    return Result.Failure("Oda bulunamadı");
                }

                var activeReservation = await _context.Reservations
                    .AnyAsync(r =>
                        r.RoomId == roomId &&
                        !r.IsDeleted &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.CheckOutDate > DateTime.UtcNow);

                if (activeReservation)
                {
                    _logger.LogWarning($"[ROOM] Aktif rezervasyon var, silinemiyor: {roomId}");
                    return Result.Failure("Aktif rezervasyonu olan odalar silinemez");
                }

                room.IsDeleted = true;
                room.UpdatedAt = DateTime.UtcNow;

                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[ROOM] Oda silindi: ID={roomId}");
                return Result.Success("Oda başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ROOM] DeleteRoomAsync hatası");
                return Result.Failure("Oda silinirken hata oluştu");
            }
        }

        public async Task<IResult<List<RoomDto>>> GetRoomsByHotelIdAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation($"[ROOM] Odalar alınıyor: Hotel={hotelId}");

                var rooms = await _context.Rooms
                    .AsNoTracking()
                    .Where(r => r.HotelId == hotelId && !r.IsDeleted && r.IsActive)
                    .ToListAsync();

                _logger.LogInformation($"[ROOM] {rooms.Count} oda bulundu");

                var roomDtos = rooms.Select(r => new RoomDto
                {
                    Id = r.Id,
                    Name = r.Name ?? "N/A",
                    Description = r.Description,
                    Capacity = r.Capacity,
                    Size = r.Size,
                    Price = r.Price,
                    IsActive = r.IsActive
                }).ToList();

                return Result<List<RoomDto>>.Success(roomDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ROOM] GetRoomsByHotelIdAsync hatası");
                return Result<List<RoomDto>>.Failure("Odalar alınırken hata oluştu");
            }
        }

        public async Task<IResult<RoomDto>> GetRoomByIdAsync(int roomId)
        {
            try
            {
                _logger.LogInformation($"[ROOM] Oda alınıyor: ID={roomId}");

                var room = await _context.Rooms
                    .AsNoTracking()
                    .Include(r => r.RoomImage)
                    .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);

                if (room == null)
                {
                    _logger.LogWarning($"[ROOM] Oda bulunamadı: {roomId}");
                    return Result<RoomDto>.Failure("Oda bulunamadı");
                }

                var roomDto = new RoomDto
                {
                    Id = room.Id,
                    Name = room.Name ?? "N/A",
                    Description = room.Description,
                    Capacity = room.Capacity,
                    Size = room.Size,
                    Price = room.Price,
                    IsActive = room.IsActive
                };

                return Result<RoomDto>.Success(roomDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ROOM] GetRoomByIdAsync hatası");
                return Result<RoomDto>.Failure("Oda alınırken hata oluştu");
            }
        }
    }
}