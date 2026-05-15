using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.Interfaces  // ← Değişti
{
    public interface IAmenityService
    {
        Task<List<AmenityDto>> GetAmenitiesByHotelIdAsync(int hotelId);
        Task<IResult> CreateAmenityAsync(int hotelId, CreateAmenityDto dto);
        Task<IResult> DeleteAmenityAsync(int id);
        Task<IResult> UpdateAmenityAsync(int id, CreateAmenityDto dto);
    }
}