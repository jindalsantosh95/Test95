using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class CreateRefundDocRepoCommand : IRequest<Unit>
    {
        public CreateRefundDocumentRequest RefundDocReq { get; set; }

        public CreateRefundDocRepoCommand(CreateRefundDocumentRequest req)
        {
            RefundDocReq = req;
        }
    }
}
