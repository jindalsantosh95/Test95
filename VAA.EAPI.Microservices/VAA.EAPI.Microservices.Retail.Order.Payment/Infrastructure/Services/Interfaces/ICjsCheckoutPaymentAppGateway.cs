using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces
{
    public interface ICjsCheckoutPaymentAppGateway
    {
        Task<CjsCheckoutPaymentAppResult> StartPaymentSessionAsync(
            CjsCheckoutPaymentAppPayload request,
            string? cookieHeader,
            CancellationToken cancellationToken);

        Task<CjsCheckoutPaymentAppResult> SetPaymentAmountAsync(
            decimal amount,
            IEnumerable<string> cookies,
            CancellationToken cancellationToken);
    }
}
