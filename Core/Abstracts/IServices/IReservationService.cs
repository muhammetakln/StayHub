using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Utils.Responses;

namespace Core.Abstracts.Interfaces  // ← Değişti
{
    public interface IReservationService
    {
        Task<List<ReservationDto>> CreateReservationAsync(int guestId, CreateReservationDto dto);
        Task<List<ReservationDto>> GetReservationsByIdAsync(int guestId);
        Task<ReservationDto?> GetReservationByIdAsync(int id);
        Task<IResult> CancelReservationAsync(int id);
        Task<IResult> UpdateReservationAsync(Reservation reservation);
    }
}