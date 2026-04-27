using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.Interfaces  // ← Değişti
{
    public interface IPaymentService
    {
        Task<IResult> CreatePaymentAsync(int reservationId,PaymentProcessDto dto);
         Task<PaymentDetailDto?> GetPaymentByReservationIdAsync(int reservationId);
        Task<IResult> UpdatePaymentStatusAsync(int id, string status);
        Task<IResult> ProcessRefundAsync(int paymentId);
    }
}