using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class CreatePaymentDocRepoCommand : IRequest<Unit>
    {
        public CreatePaymentDocumentRequest PaymentDocReq { get; set; }

        public CreatePaymentDocRepoCommand(CreatePaymentDocumentRequest req)
        {
            PaymentDocReq = req;
        }
    }
}
