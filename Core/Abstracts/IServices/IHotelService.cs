using Core.Concretes.DTOs;


namespace Core.Abstracts.IServices
{
    public interface IHotelService
    {
        Task<List<HotelDto>> GetHotelsAsync ();
        Task<HotelDetailDto> GetHotelByIdAsync (int id);
        Task<int> CreateHotelAsync(CreateHotelDto dto);
        Task UpdateHotelAsync(int id, UpdateHotelDto dto);
        Task DeleteHotelAsync(int id);
        Task<List<HotelDto>> SearchHotelsAsync(string searchTerm);
        Task<List<HotelDto>> GetHotelsByCityAsync(string city);
        Task<List<HotelDto>> GetHotelsByRatingAsync( decimal minRating );
        Task<List<HotelDto>> FilterHotelsAsync(HotelSearchFilterDto dto);
        Task<bool> IsHotelExistsAsync(int id);
    }
}
