using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PaymentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate
{
    public class PaymentUpdateRepoCommand : IRequest<Unit>
    {
        public PaymentUpdateRequestData PaymentUpdateRequestData { get; set; }

        public PaymentUpdateRepoCommand(PaymentUpdateRequestData paymentUpdateRequestData)
        {
            PaymentUpdateRequestData = paymentUpdateRequestData;
        }
    }
}
