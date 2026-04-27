using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.Interfaces  // ← Değişti
{
    public interface IReviewService
    {
        Task<IResult<ReviewDto>> CreateReviewAsync(int guestId,CreateReviewDto dto);
        
        Task<ReviewListDto?> GetReviewByIdAsync(int id, ReviewListDto dto);
        Task<IResult> UpdateReviewAsync(int id,UpdateReviewDto dto);
        Task<IResult> DeleteReviewAsync(int id);
    }
}