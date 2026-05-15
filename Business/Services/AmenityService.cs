using AutoMapper;
using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Responses;

namespace Business.Services
{
    public class AmenityService:IAmenityService
    {
        private readonly StayHubContext context;
        private readonly ILogger<AmenityService> logger;
        private readonly IMapper mapper;

        public AmenityService(StayHubContext context, ILogger<AmenityService> logger, IMapper mapper)
        {
            this.context = context;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task<IResult> CreateAmenityAsync(int hotelId, CreateAmenityDto dto)
        {
            try
            {
                logger.LogInformation($"[AMENITY] Olanak oluşturuluyor: Hotel={hotelId}, Name={dto.Name}");

                var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId && !h.IsDeleted);
                if (!hotelExists)
                {
                   logger.LogWarning($"[AMENITY] Otel bulunamadı: {hotelId}");
                    return Result.Failure("Otel bulunamadı");
                }

                var amenityExists = await context.Amenities
                    .AnyAsync(a => a.HotelId == hotelId && a.Name == dto.Name && !a.IsDeleted);

                if (amenityExists)
                {
                    logger.LogWarning($"[AMENITY] Olanak zaten var: {dto.Name}");
                    return Result.Failure("Bu olanak zaten mevcut");
                }

                var amenity = mapper.Map<Amenity>(dto);
                amenity.HotelId = hotelId;
                amenity.CreatedAt = DateTime.UtcNow;

                await context.Amenities.AddAsync(amenity);
                await context.SaveChangesAsync();

                logger.LogInformation($"[AMENITY] Olanak oluşturuldu: ID={amenity.Id}");
                return Result.Success("Olanak başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[AMENITY] CreateAmenityAsync hatası");
                return Result.Failure("Olanak oluşturulurken hata oluştu");
            }
        }

        public async Task<IResult> DeleteAmenityAsync(int id)
        {
            try
            {
                logger.LogInformation($"[AMENITY] Olanak siliniyor: ID={id}");

                var amenity = await context.Amenities.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
                if (amenity == null)
                {
                    logger.LogWarning($"[AMENITY] Olanak bulunamadı: {id}");
                    return Result.Failure("Olanak bulunamadı");
                }

                amenity.IsDeleted = true;
                amenity.UpdatedAt = DateTime.UtcNow;

                context.Amenities.Update(amenity);
                await context.SaveChangesAsync();

                logger.LogInformation($"[AMENITY] Olanak silindi: ID={id}");
                return Result.Success("Olanak başarıyla silindi");
            }
            catch (Exception ex)
            {
               logger.LogError(ex, "[AMENITY] DeleteAmenityAsync hatası");
                return Result.Failure("Olanak silinirken hata oluştu");
            }
        }

        public async Task<List<AmenityDto>> GetAmenitiesByHotelIdAsync(int hotelId)
        {
            try
            {
                logger.LogInformation($"[AMENITY] Olanaklar alınıyor: Hotel={hotelId}");

                var amenities = await context.Amenities
                    .AsNoTracking()
                    .Where(a => a.HotelId == hotelId && !a.IsDeleted)
                    .ToListAsync();

                logger.LogInformation($"[AMENITY] {amenities.Count} olanak bulundu");
                return mapper.Map<List<AmenityDto>>(amenities);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[AMENITY] GetAmenitiesByHotelIdAsync hatası");
                return new List<AmenityDto>();
            }
        }
        public async Task<IResult> UpdateAmenityAsync(int id, CreateAmenityDto dto)
        {
            try
            {
                var amenity = await context.Amenities.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
                if (amenity == null) return Result.Failure("Olanak bulunamadı");

                mapper.Map(dto, amenity);
                amenity.UpdatedAt = DateTime.UtcNow;

                context.Amenities.Update(amenity);
                await context.SaveChangesAsync();
                return Result.Success("Olanak güncellendi");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[AMENITY] UpdateAmenityAsync hatası");
                return Result.Failure("Güncelleme sırasında hata oluştu");
            }
        }
    }
}
