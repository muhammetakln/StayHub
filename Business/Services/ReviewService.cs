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
                logger.LogInformation($"[REVIEW] Yorum yazılıyor:Guest={guestId},Hotel={dto.HotelId}");
                var hotelExists = await context.Hotels.AnyAsync(h => h.Id == dto.HotelId && !h.IsDeleted);
                if (!hotelExists)
                {
                    logger.LogWarning($"[REVIEW] Yorum yazılamadı:Hotel bulunamadı:Hotel={dto.HotelId}");
                    return Result<ReviewDto>.Failure("Otel bulunamadı");
                }
                var review = mapper.Map<Review>(dto);
                review.GuestId = guestId;
                review.CreatedAt = DateTime.UtcNow;
                review.IsPublished = true;
                review.IsDeleted = false;

                context.Reviews.Add(review);
                await context.SaveChangesAsync();

                return Result<ReviewDto>.Success(mapper.Map<ReviewDto>(review), "Yorum başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CreateReviewAsync hatası");
                return Result<ReviewDto>.Failure("Yorum eklenirken hata oluştu");
            }
        }

        public async Task<ReviewListDto?> GetReviewByIdAsync(int id, ReviewListDto dto)
        {
            try
            {
                var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
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

        // ✅ Controller'dan buraya taşınan yeni mimari metotlar:
        public async Task<IResult> AddReviewAsync(int hotelId, int guestId, int rating, string title, string content)
        {
            try
            {
                var review = new Review
                {
                    HotelId = hotelId,
                    GuestId = guestId,
                    Rating = rating,
                    Title = title,
                    Comment = content,
                    CreatedAt = DateTime.UtcNow,
                    IsPublished = true,
                    IsDeleted = false
                };

                context.Reviews.Add(review);
                await context.SaveChangesAsync();

                return Result.Success("Yorum eklendi.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddReviewAsync hatası");
                return Result.Failure("Yorum eklenemedi.");
            }
        }

        public async Task<IResult> DeleteReviewAsync(int id)
        {
            try
            {
                var review = await context.Reviews.FindAsync(id);
                if (review == null) return Result.Failure("Yorum bulunamadı.");

                review.IsDeleted = true; // Soft delete
                await context.SaveChangesAsync();

                return Result.Success("Yorum silindi.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DeleteReviewAsync hatası");
                return Result.Failure("Yorum silinemedi.");
            }
        }

        public async Task<IResult> ReplyReviewAsync(int reviewId, string replyText)
        {
            try
            {
                var review = await context.Reviews.FindAsync(reviewId);
                if (review == null) return Result.Failure("Yorum bulunamadı.");

                review.OwnerReply = replyText;
                review.OwnerReplyDate = DateTime.UtcNow;
                review.IsReplied = true;

                await context.SaveChangesAsync();

                return Result.Success("Yanıtlama başarılı.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ReplyReviewAsync hatası");
                return Result.Failure("Yanıt eklenemedi.");
            }
        }
    }
}