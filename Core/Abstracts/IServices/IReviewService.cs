using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.Interfaces
{
    public interface IReviewService
    {
        Task<IResult<ReviewDto>> CreateReviewAsync(int guestId, CreateReviewDto dto);
        Task<ReviewListDto?> GetReviewByIdAsync(int id, ReviewListDto dto);
        Task<IResult> UpdateReviewAsync(int id, UpdateReviewDto dto);

        Task<IResult> AddReviewAsync(int hotelId, int guestId, int rating, string title, string content);
        Task<IResult> DeleteReviewAsync(int id);
        Task<IResult> ReplyReviewAsync(int reviewId, string replyText);
    }
}