using AutoMapper;
using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Responses;

namespace Business.Services
{
    public class ReviewService : IReviewService
    {
        private readonly StayHubContext context;
        private readonly ILogger<ReviewService> logger;
        private readonly IMapper mapper;

        public ReviewService(StayHubContext context, ILogger<ReviewService> logger, IMapper mapper)
        {
            this.context = context;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task<IResult<ReviewDto>> CreateReviewAsync(int guestId, CreateReviewDto dto)
        {
            try
            {
                logger.LogInformation($"[REVİEW] Yorum yazılıyor:Guest={guestId},Hotel={dto.HotelId}");
                var hotelExists = await context.Hotels.AnyAsync(h => h.Id == dto.HotelId && !h.IsDeleted);
                if (!hotelExists)
                {
                    logger.LogWarning($"[REVİEW] Yorum yazılamadı:Hotel bulunamadı:Hotel={dto.HotelId}");
                    return Result<ReviewDto>.Failure("Hotel bulunamadı", statusCode: 404);
                }
                var review = mapper.Map<Review>(dto);
                review.GuestId = guestId;
                review.CreatedAt = DateTime.UtcNow;
                await context.Reviews.AddAsync(review);
                await context.SaveChangesAsync();
                logger.LogInformation($"[REVİEW] Yorum başarıyla yazıldı:ReviewId={review.Id}");
                return Result<ReviewDto>.Success(mapper.Map<ReviewDto>(review));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[REVİEW] Yorum yazılırken bir hata oluştu");
                return Result<ReviewDto>.Failure("Yorum yazılırken bir hata oluştu");
            }
        }

        public async Task<IResult> DeleteReviewAsync(int id)
        {
            try
            {
                logger.LogInformation($"[REVIEW] Yorum siliniyor: ID={id}");
                var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
                if (review == null)
                {
                    logger.LogWarning($"[REVIEW] Yorum bulunamadı: {id}");
                    return Result.Failure("Yorum bulunamadı");
                }

                review.IsDeleted = true;
                review.UpdatedAt = DateTime.UtcNow;

                context.Reviews.Update(review);
                await context.SaveChangesAsync();

                logger.LogInformation($"[REVIEW] Yorum silindi: ID={id}");
                return Result.Success("Yorum silindi");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[REVIEW] DeleteReviewAsync hatası");
                return Result.Failure("Yorum silinirken hata oluştu");
            }
        }

       
        public async Task<ReviewListDto?> GetReviewByIdAsync(int id, ReviewListDto dto)
        {
            try
            {
                logger.LogInformation($"[REVIEW] Tekil yorum alınıyor: ID={id}");

                var review = await context.Reviews
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (review == null)
                {
                    logger.LogWarning($"[REVIEW] Yorum bulunamadı: {id}");
                    return null;
                }

                return mapper.Map<ReviewListDto>(review);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[REVIEW] GetReviewByIdAsync hatası");
                return null;
            }
        }

        public async Task<IResult> UpdateReviewAsync(int id, UpdateReviewDto dto)
        {
            try
            {
                logger.LogInformation($"[REVIEW] Yorum güncelleniyor: ID={id}");
                var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
                if (review == null)
                {
                    logger.LogWarning($"[REVIEW] Yorum bulunamadı: {id}");
                    return Result.Failure("Yorum bulunamadı");
                }

                mapper.Map(dto, review);
                review.UpdatedAt = DateTime.UtcNow;

                context.Reviews.Update(review);
                await context.SaveChangesAsync();

                logger.LogInformation($"[REVIEW] Yorum güncellendi: ID={id}");
                return Result.Success("Yorum güncellendi");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[REVIEW] UpdateReviewAsync hatası");
                return Result.Failure("Yorum güncellenirken hata oluştu");
            }
        }
    }

}
