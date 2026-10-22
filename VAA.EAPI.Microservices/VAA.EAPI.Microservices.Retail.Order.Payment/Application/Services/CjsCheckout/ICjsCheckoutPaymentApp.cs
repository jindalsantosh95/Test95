using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Services.CjsCheckout
{
    public interface ICjsCheckoutPaymentApp
    {
        Task<Result<Response<CjsCheckoutPaymentAppResponse>>> HandleAsync(
            string requestBody,
            CancellationToken cancellationToken);
    }
}
