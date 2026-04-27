using AutoMapper;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class HotelService : IHotelService
    {
        private readonly StayHubContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<HotelService> _logger;

        public HotelService(StayHubContext context, IMapper mapper, ILogger<HotelService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<int> CreateHotelAsync(CreateHotelDto dto)
        {
            try
            {
                _logger.LogInformation($"Yeni otel oluşturuluyor: {dto.Name}");
                var hotel = _mapper.Map<Hotel>(dto);
                hotel.CreatedAt = DateTime.UtcNow;
                hotel.IsActive = true;
                hotel.IsDeleted = false;
                await _context.Hotels.AddAsync(hotel);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Otel oluşturuldu. ID: {hotel.Id}");
                return hotel.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateHotelAsync hatası");
                throw;
            }
        }

        public async Task DeleteHotelAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Otel siliniyor: {id}");
                var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);
                if (hotel == null)
                {
                    throw new KeyNotFoundException($"Otel bulunamadı: {id}");
                }

                hotel.IsDeleted = true;
                hotel.UpdatedAt = DateTime.UtcNow;
                _context.Hotels.Update(hotel);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Otel silindi: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteHotelAsync hatası");
                throw;
            }
        }

        public async Task<List<HotelDto>> FilterHotelsAsync(HotelFilterDto dto)
        {
            try
            {
                _logger.LogInformation("Oteller filtreleniyor");
                var query = _context.Hotels
                    .AsNoTracking()
                    .Where(h => !h.IsDeleted && (dto.IsActive == null || h.IsActive == dto.IsActive));

                if (!string.IsNullOrEmpty(dto.Name))
                    query = query.Where(h => h.Name.Contains(dto.Name));

                if (!string.IsNullOrEmpty(dto.City))
                    query = query.Where(h => h.City == dto.City);

                if (!string.IsNullOrEmpty(dto.Region))
                    query = query.Where(h => h.Region == dto.Region);

                if (!string.IsNullOrEmpty(dto.Country))
                    query = query.Where(h => h.Country == dto.Country);

                if (dto.MinRating.HasValue)
                {
                    var minRating = decimal.Parse(dto.MinRating.ToString());
                    query = query.Where(h => decimal.Parse(h.Rating ?? "0") >= minRating);
                }

                if (dto.MaxRating.HasValue)
                {
                    var maxRating = decimal.Parse(dto.MaxRating.ToString());
                    query = query.Where(h => decimal.Parse(h.Rating ?? "0") <= maxRating);
                }

                if (!string.IsNullOrEmpty(dto.HotelType))
                    query = query.Where(h => h.HotelType == dto.HotelType);

                var sortBy = dto.SortBy?.ToLower() ?? "name";
                var sortOrder = dto.SortOrder?.ToLower() ?? "asc";

                if (sortBy == "rating")
                {
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(h => decimal.Parse(h.Rating ?? "0"))
                        : query.OrderBy(h => decimal.Parse(h.Rating ?? "0"));
                }
                else
                {
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(h => h.Name)
                        : query.OrderBy(h => h.Name);
                }

                var hotels = await query
                    .Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .ToListAsync();

                return _mapper.Map<List<HotelDto>>(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FilterHotelsAsync hatası");
                throw;
            }
        }

       
        public async Task<HotelDetailDto> GetHotelByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Otel bulunuyor: {id}");
                var hotel = await _context.Hotels
                    .AsNoTracking()
                    .Include(h => h.Rooms)
                    .Include(h => h.Amenities)      // ✅ EKLENDI
                    .Include(h => h.Reviews)        // ✅ EKLENDI
                    .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

                if (hotel == null)
                {
                    _logger.LogWarning($"Otel bulunamadı: {id}");
                    throw new KeyNotFoundException($"Otel bulunamadı: {id}");
                }

                _logger.LogInformation($"Otel başarıyla yüklendi: {hotel.Name}");
                return _mapper.Map<HotelDetailDto>(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetHotelByIdAsync hatası: {id}");
                throw;
            }
        }
        

        public async Task<List<HotelDto>> GetHotelsAsync()
        {
            try
            {
                _logger.LogInformation("Tüm oteller getiriliyor");
                var hotels = await _context.Hotels
                    .AsNoTracking()
                    .Where(h => !h.IsDeleted && h.IsActive)
                    .ToListAsync();

                _logger.LogInformation($"Toplam {hotels.Count} otel getirildi");
                return _mapper.Map<List<HotelDto>>(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetHotelsAsync hatası");
                throw;
            }
        }

        public async Task<List<HotelDto>> GetHotelsByCityAsync(string city)
        {
            try
            {
                _logger.LogInformation($"Şehirdeki oteller getiriliyor: {city}");
                var hotels = await _context.Hotels
                    .AsNoTracking()
                    .Where(h => !h.IsDeleted && h.IsActive && h.City == city)
                    .OrderByDescending(h => h.Rating)
                    .ToListAsync();

                return _mapper.Map<List<HotelDto>>(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetHotelsByCityAsync hatası");
                throw;
            }
        }

        // ✅ FIXED: Database'de filtreleme yap
        public async Task<List<HotelDto>> GetHotelsByRatingAsync(decimal minRating)
        {
            try
            {
                _logger.LogInformation($"Oteller listeleniyor (min puan: {minRating})");

                var hotels = await _context.Hotels
                    .AsNoTracking()
                    .Where(h => !h.IsDeleted && h.IsActive &&
                           decimal.Parse(h.Rating ?? "0") >= minRating)  // ← DATABASE'DE
                    .OrderByDescending(h => decimal.Parse(h.Rating ?? "0"))
                    .ToListAsync();

                return _mapper.Map<List<HotelDto>>(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetHotelsByRatingAsync hatası");
                throw;
            }
        }

        public async Task<bool> IsHotelExistsAsync(int id)
        {
            try
            {
                return await _context.Hotels
                    .AnyAsync(h => h.Id == id && !h.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IsHotelExistsAsync hatası");
                throw;
            }
        }

        public async Task<List<HotelDto>> SearchHotelsAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation($"Otel aranıyor: {searchTerm}");
                var hotels = await _context.Hotels
                    .AsNoTracking()
                    .Where(h => !h.IsDeleted && h.IsActive &&
                    (h.Name.Contains(searchTerm) ||
                    h.City.Contains(searchTerm) ||
                    h.Address.Contains(searchTerm)))
                    .ToListAsync();

                return _mapper.Map<List<HotelDto>>(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Otel araması başarısız: {searchTerm}");
                throw;
            }
        }

        public async Task UpdateHotelAsync(int id, UpdateHotelDto dto)
        {
            try
            {
                _logger.LogInformation($"Otel güncelleniyor: {id}");
                var hotel = await _context.Hotels
                    .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

                if (hotel == null)
                    throw new KeyNotFoundException($"Otel bulunamadı: {id}");

                _mapper.Map(dto, hotel);
                hotel.UpdatedAt = DateTime.UtcNow;
                _context.Hotels.Update(hotel);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Otel güncellendi: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel güncellenme hatası");
                throw;
            }
        }
    }
}