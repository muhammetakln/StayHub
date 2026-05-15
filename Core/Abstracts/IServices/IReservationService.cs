using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Utils.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Abstracts.Interfaces
{
    public interface IReservationService
    {
        Task<IResult<List<ReservationDto>>> CreateReservationAsync(int guestId, CreateReservationDto dto);

        Task SendInvoiceEmail(Guest guest, Reservation reservation, Room room);
        Task<List<ReservationDto>> GetReservationsByIdAsync(int guestId);
        Task<ReservationDto?> GetReservationByIdAsync(int id);
        Task<IResult> CancelReservationAsync(int id);
        Task<IResult> UpdateReservationAsync(Reservation reservation);
        Task SendCancellationEmail(int reservationId);
        Task<decimal> GetMonthlyRevenueByHotelIdAsync(int hotelId);
    }
}