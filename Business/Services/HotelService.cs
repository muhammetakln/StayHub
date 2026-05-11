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

        public async Task<List<HotelDto>> FilterHotelsAsync(HotelSearchFilterDto dto)
        {
            try
            {
                _logger.LogInformation("Oteller filtreleniyor");

                var query = _context.Hotels
                    .AsNoTracking()
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews)
                    .Include(h => h.AddOnServices)
                    .Where(h => !h.IsDeleted);

                if (!string.IsNullOrEmpty(dto.SearchKeyword))
                {
                    query = query.Where(h => h.Name.Contains(dto.SearchKeyword) ||
                                               h.City.Contains(dto.SearchKeyword) ||
                                               h.Country.Contains(dto.SearchKeyword));
                }

                if (!string.IsNullOrEmpty(dto.City))
                    query = query.Where(h => h.City == dto.City);

                if (!string.IsNullOrEmpty(dto.Country))
                    query = query.Where(h => h.Country == dto.Country);

                if (dto.MinStarRating.HasValue)
                    query = query.Where(h => h.StarRating >= dto.MinStarRating.Value);

                if (dto.MinPrice.HasValue)
                {
                    query = query.Where(h => h.Rooms.Any() && h.Rooms.Min(r => r.Price) >= dto.MinPrice.Value);
                }

                if (dto.MaxPrice.HasValue)
                {
                    query = query.Where(h => h.Rooms.Any() && h.Rooms.Min(r => r.Price) <= dto.MaxPrice.Value);
                }

                var sortBy = dto.SortBy?.ToLower() ?? "name";

                if (sortBy == "rating_desc")
                {
                    query = query.OrderByDescending(h => h.Reviews.Average(r => (double?)r.Rating) ?? 0);
                }
                else if (sortBy == "price_asc")
                {
                    query = query.OrderBy(h => h.Rooms.Min(r => (decimal?)r.Price) ?? 0);
                }
                else
                {
                    query = query.OrderBy(h => h.Name);
                }

                var hotels = await query
                    .Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .ToListAsync();

                // ✅ GÜNCELLEME: Dinamik puanı hesapla
                return CalculateDynamicRatings(hotels);
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
                    .Include(h => h.Amenities)
                    .Include(h => h.Reviews)
                    .Include(h => h.AddOnServices)
                    .Include(h => h.Rooms.Where(r => !r.IsDeleted))
                        .ThenInclude(r => r.RoomImage)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

                if (hotel == null)
                {
                    _logger.LogWarning($"Otel bulunamadı: {id}");
                    throw new KeyNotFoundException($"Otel bulunamadı: {id}");
                }

                _logger.LogInformation($"Otel başarıyla yüklendi: {hotel.Name}");

                var dto = _mapper.Map<HotelDetailDto>(hotel);

                // ✅ GÜNCELLEME: Detay sayfası için aktif yorumların puan ortalamasını hesapla
                var activeReviews = hotel.Reviews?.Where(r => !r.IsDeleted).ToList();
                if (activeReviews != null && activeReviews.Any())
                {
                    dto.AverageRating = Math.Round(activeReviews.Average(r => (double)r.Rating), 1);
                    dto.ReviewCount = activeReviews.Count;
                }
                else
                {
                    dto.AverageRating = 0;
                    dto.ReviewCount = 0;
                }

                return dto;
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
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews)
                    .Include(h => h.AddOnServices)
                    .Where(h => !h.IsDeleted && h.IsActive)
                    .ToListAsync();

                _logger.LogInformation($"Toplam {hotels.Count} otel getirildi");

                // ✅ GÜNCELLEME: Dinamik puanı hesapla
                return CalculateDynamicRatings(hotels);
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
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews)
                    .Include(h => h.AddOnServices)
                    .Where(h => !h.IsDeleted && h.IsActive && h.City == city)
                    .OrderByDescending(h => h.Reviews.Average(r => (double?)r.Rating) ?? 0)
                    .ToListAsync();

                // ✅ GÜNCELLEME: Dinamik puanı hesapla
                return CalculateDynamicRatings(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetHotelsByCityAsync hatası");
                throw;
            }
        }

        public async Task<List<HotelDto>> GetHotelsByRatingAsync(decimal minRating)
        {
            try
            {
                _logger.LogInformation($"Oteller listeleniyor (min puan: {minRating})");

                var hotels = await _context.Hotels
                    .AsNoTracking()
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews)
                    .Include(h => h.AddOnServices)
                    .Where(h => !h.IsDeleted && h.IsActive &&
                           (h.Reviews.Average(r => (double?)r.Rating) ?? 0) >= (double)minRating)
                    .OrderByDescending(h => h.Reviews.Average(r => (double?)r.Rating) ?? 0)
                    .ToListAsync();

                // ✅ GÜNCELLEME: Dinamik puanı hesapla
                return CalculateDynamicRatings(hotels);
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
            var filter = new HotelSearchFilterDto { SearchKeyword = searchTerm };
            return await FilterHotelsAsync(filter);
        }

        public async Task UpdateHotelAsync(int id, UpdateHotelDto dto)
        {
            try
            {
                var hotel = await _context.Hotels
                    .Include(h => h.AddOnServices)
                    .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

                if (hotel == null)
                    throw new KeyNotFoundException($"Otel bulunamadı: {id}");

                // Temel bilgileri güncelle
                _mapper.Map(dto, hotel);

                // ✅ EK HİZMETLERİ AKILLI GÜNCELLEME (Price Değişim Kontrolü Eklendi)
                if (dto.AddOnServices != null)
                {
                    var dtoIds = dto.AddOnServices.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                    var servicesToRemove = hotel.AddOnServices.Where(s => !dtoIds.Contains(s.Id)).ToList();

                    foreach (var s in servicesToRemove)
                    {
                        bool isUsed = await _context.ReservationAddOnServices.AnyAsync(ra => ra.AddOnServiceId == s.Id);
                        if (isUsed) { s.IsDeleted = true; s.IsActive = false; }
                        else { _context.AddOnServices.Remove(s); }
                    }

                    foreach (var sDto in dto.AddOnServices.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                    {
                        if (sDto.Id > 0)
                        {
                            var existing = hotel.AddOnServices.FirstOrDefault(x => x.Id == sDto.Id);
                            if (existing != null)
                            {
                                // ✅ Sadece bir veri değişmişse UPDATE yap ve fiyatı direkt ata (Convert kullanma)
                                if (existing.Name != sDto.Name || existing.Price != sDto.Price || existing.Unit != sDto.Unit)
                                {
                                    existing.Name = sDto.Name;
                                    existing.Price = sDto.Price;
                                    existing.Unit = sDto.Unit;
                                    existing.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                        }
                        else
                        {
                            hotel.AddOnServices.Add(new AddOnService
                            {
                                Name = sDto.Name,
                                Price = sDto.Price,
                                Unit = sDto.Unit,
                                HotelId = hotel.Id,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                hotel.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Otel ID {id} başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateHotelAsync hatası");
                throw;
            }
        }

        // ✅ YENİ: Listelerde puanı dinamik hesaplayan yardımcı metod
        private List<HotelDto> CalculateDynamicRatings(List<Hotel> hotels)
        {
            var dtos = _mapper.Map<List<HotelDto>>(hotels);
            foreach (var dto in dtos)
            {
                var entity = hotels.First(h => h.Id == dto.Id);
                var activeReviews = entity.Reviews?.Where(r => !r.IsDeleted).ToList();

                if (activeReviews != null && activeReviews.Any())
                {
                    dto.AverageRating = Math.Round(activeReviews.Average(r => (double)r.Rating), 1);
                }
                else
                {
                    dto.AverageRating = 0;
                }
            }
            return dtos;
        }
    }
}