using Core.Concretes.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Responses;

namespace Core.Abstracts.Interfaces
{
    public interface IRoomService
    {
        // CRUD İşlemleri
        Task<IResult<RoomDto>> CreateRoomByIdAsync(int hotelId, CreateRoomDto dto);
        Task<IResult> UpdateRoomAsync(int roomId, UpdateRoomDto dto);
        Task<IResult> DeleteRoomAsync(int roomId);

        // Veri Okuma İşlemleri
        Task<IResult<List<RoomDto>>> GetRoomsByHotelIdAsync(int hotelId);
        Task<IResult<RoomDto>> GetRoomByIdAsync(int roomId);
    }
}