using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Utils.Responses;

namespace Core.Abstracts
{
    public interface IUnitOfWork : IAsyncDisposable
    {

        IHotelRepository HotelRepository { get; }
        IAmenityRepository AmenityRepository { get; }
        IAddOnServiceRepository AddOnServiceRepository { get; }
        IRoomImageRepository RoomImageRepository { get; }
        IRoomRepository RoomRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        IReviewRepository ReviewRepository { get; }
        IReservationAddOnServiceRepository ReservationAddOnServiceRepository { get; }
        IReservationRepository ReservationRepository { get; }
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task<IResult> CommitAsync();
        Task RollbackAsync();
        Task CloseConnectionAsync();
    }
}
