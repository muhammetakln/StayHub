using AutoMapper;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Business.Services
{
    public class HotelService : IHotelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<HotelService> _logger;

        public HotelService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<HotelService> logger)
        {
            _unitOfWork = unitOfWork;
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

                await _unitOfWork.HotelRepository.AddAsync(hotel);
                await _unitOfWork.SaveChangesAsync();

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

                var hotel = await _unitOfWork.HotelRepository.GetFirstAsync(h => h.Id == id);
                if (hotel == null)
                {
                    throw new KeyNotFoundException($"Otel bulunamadı: {id}");
                }

                hotel.IsDeleted = true;
                hotel.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.HotelRepository.UpdateAsync(hotel);
                await _unitOfWork.SaveChangesAsync();

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

                var hotelsEnum = await _unitOfWork.HotelRepository.GetManyAsync(
                    h => !h.IsDeleted,
                    "Rooms", "Reviews", "AddOnServices");

                var query = hotelsEnum.AsQueryable();

                if (!string.IsNullOrEmpty(dto.SearchKeyword))
                {
                    query = query.Where(h => h.Name.Contains(dto.SearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                                               h.City.Contains(dto.SearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                                               h.Country.Contains(dto.SearchKeyword, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(dto.City))
                    query = query.Where(h => h.City.Equals(dto.City, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(dto.Country))
                    query = query.Where(h => h.Country.Equals(dto.Country, StringComparison.OrdinalIgnoreCase));

                if (dto.MinStarRating.HasValue)
                    query = query.Where(h => h.StarRating >= dto.MinStarRating.Value);

                if (dto.MinPrice.HasValue)
                {
                    query = query.Where(h => h.Rooms != null && h.Rooms.Any() && h.Rooms.Min(r => r.Price) >= dto.MinPrice.Value);
                }

                if (dto.MaxPrice.HasValue)
                {
                    query = query.Where(h => h.Rooms != null && h.Rooms.Any() && h.Rooms.Min(r => r.Price) <= dto.MaxPrice.Value);
                }

                var sortBy = dto.SortBy?.ToLower() ?? "name";

                if (sortBy == "rating_desc")
                {
                    // ✅ HATA ÇÖZÜMÜ: Expression Tree içerisinde "?. " (null propagation) yerine açık kontrol yazıldı.
                    query = query.OrderByDescending(h => (h.Reviews != null && h.Reviews.Any()) ? h.Reviews.Average(r => (double)r.Rating) : 0);
                }
                else if (sortBy == "price_asc")
                {
                    // ✅ HATA ÇÖZÜMÜ
                    query = query.OrderBy(h => (h.Rooms != null && h.Rooms.Any()) ? h.Rooms.Min(r => r.Price) : 0);
                }
                else
                {
                    query = query.OrderBy(h => h.Name);
                }

                var hotels = query
                    .Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .ToList();

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

                var hotel = await _unitOfWork.HotelRepository.GetFirstAsync(
                    h => h.Id == id && !h.IsDeleted,
                    "Amenities", "Reviews", "AddOnServices", "Rooms", "Rooms.RoomImage");

                if (hotel == null)
                {
                    _logger.LogWarning($"Otel bulunamadı: {id}");
                    throw new KeyNotFoundException($"Otel bulunamadı: {id}");
                }

                if (hotel.Rooms != null)
                {
                    hotel.Rooms = hotel.Rooms.Where(r => !r.IsDeleted).ToList();
                }

                _logger.LogInformation($"Otel başarıyla yüklendi: {hotel.Name}");

                var dto = _mapper.Map<HotelDetailDto>(hotel);

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

                var hotels = await _unitOfWork.HotelRepository.GetManyAsync(
                    h => !h.IsDeleted && h.IsActive,
                    "Rooms", "Reviews", "AddOnServices");

                _logger.LogInformation($"Toplam {hotels.Count()} otel getirildi");

                return CalculateDynamicRatings(hotels.ToList());
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

                var hotelsEnum = await _unitOfWork.HotelRepository.GetManyAsync(
                    h => !h.IsDeleted && h.IsActive && h.City == city,
                    "Rooms", "Reviews", "AddOnServices");

                // ✅ HATA ÇÖZÜMÜ
                var hotels = hotelsEnum
                    .OrderByDescending(h => (h.Reviews != null && h.Reviews.Any()) ? h.Reviews.Average(r => (double)r.Rating) : 0)
                    .ToList();

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

                var hotelsEnum = await _unitOfWork.HotelRepository.GetManyAsync(
                    h => !h.IsDeleted && h.IsActive,
                    "Rooms", "Reviews", "AddOnServices");

                // ✅ HATA ÇÖZÜMÜ
                var hotels = hotelsEnum
                    .Where(h => ((h.Reviews != null && h.Reviews.Any()) ? h.Reviews.Average(r => (double)r.Rating) : 0) >= (double)minRating)
                    .OrderByDescending(h => (h.Reviews != null && h.Reviews.Any()) ? h.Reviews.Average(r => (double)r.Rating) : 0)
                    .ToList();

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
                return await _unitOfWork.HotelRepository.AnyAsync(h => h.Id == id && !h.IsDeleted);
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
                await _unitOfWork.BeginTransactionAsync();

                var hotel = await _unitOfWork.HotelRepository.GetFirstAsync(
                    h => h.Id == id && !h.IsDeleted,
                    "AddOnServices");

                if (hotel == null)
                    throw new KeyNotFoundException($"Otel bulunamadı: {id}");

                _mapper.Map(dto, hotel);

                if (dto.AddOnServices != null)
                {
                    var dtoIds = dto.AddOnServices.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                    var servicesToRemove = hotel.AddOnServices?.Where(s => !dtoIds.Contains(s.Id)).ToList() ?? new List<AddOnService>();

                    foreach (var s in servicesToRemove)
                    {
                        bool isUsed = await _unitOfWork.ReservationAddOnServiceRepository.AnyAsync(ra => ra.AddOnServiceId == s.Id);

                        if (isUsed)
                        {
                            s.IsDeleted = true;
                            s.IsActive = false;
                            await _unitOfWork.AddOnServiceRepository.UpdateAsync(s);
                        }
                        else
                        {
                            await _unitOfWork.AddOnServiceRepository.DeleteAsync(s);
                        }
                    }

                    foreach (var sDto in dto.AddOnServices.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                    {
                        if (sDto.Id > 0)
                        {
                            var existing = hotel.AddOnServices?.FirstOrDefault(x => x.Id == sDto.Id);
                            if (existing != null)
                            {
                                if (existing.Name != sDto.Name || existing.Price != sDto.Price || existing.Unit != sDto.Unit)
                                {
                                    existing.Name = sDto.Name;
                                    existing.Price = sDto.Price;
                                    existing.Unit = sDto.Unit;
                                    existing.UpdatedAt = DateTime.UtcNow;

                                    await _unitOfWork.AddOnServiceRepository.UpdateAsync(existing);
                                }
                            }
                        }
                        else
                        {
                            var newService = new AddOnService
                            {
                                Name = sDto.Name,
                                Price = sDto.Price,
                                Unit = sDto.Unit,
                                HotelId = hotel.Id,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };

                            hotel.AddOnServices?.Add(newService);
                            await _unitOfWork.AddOnServiceRepository.AddAsync(newService);
                        }
                    }
                }

                hotel.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.HotelRepository.UpdateAsync(hotel);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Otel ID {id} başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "UpdateHotelAsync hatası");
                throw;
            }
        }

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