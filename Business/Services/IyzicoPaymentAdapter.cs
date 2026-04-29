using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
// using Iyzipay.Request; (NuGet'ten Iyzipay kurulduğunda açılacak)
// using Iyzipay.Model;

namespace Business.Adapters
{
    public class IyzicoPaymentAdapter : IPaymentAdapter
    {
        private readonly IConfiguration _configuration;
        public string ProviderName => "Iyzico";

        public IyzicoPaymentAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<PaymentResultDto> ProcessPaymentAsync(PaymentProcessDto request)
        {
            await Task.Delay(1000); // API simülasyonu
            return new PaymentResultDto { IsSuccess = true, TransactionId = "IYZ-" + System.Guid.NewGuid().ToString().Substring(0, 8) };
        }
    }
}