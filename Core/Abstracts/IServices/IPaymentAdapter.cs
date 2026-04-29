using Core.Concretes.DTOs;

namespace Core.Abstracts.Interfaces  // ← Değişti
{
    public interface IPaymentAdapter
    {
        string ProviderName { get; } // "Iyzico", "Stripe", "GarantiPos" gibi
        Task<PaymentResultDto> ProcessPaymentAsync(PaymentProcessDto request);
    }
    public class StripePaymentAdapter : IPaymentAdapter
    {
        public string ProviderName => "Stripe";

       
        public async Task<PaymentResultDto> ProcessPaymentAsync(PaymentProcessDto request)
        {
            await Task.Delay(1000);
            return new PaymentResultDto { IsSuccess = true, TransactionId = "STR-" + System.Guid.NewGuid() };
        }
    }
}