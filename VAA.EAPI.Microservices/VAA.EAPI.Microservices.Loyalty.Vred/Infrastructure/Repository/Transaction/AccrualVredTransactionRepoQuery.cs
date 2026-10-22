using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction
{
    public class AccrualVredTransactionRepoQuery : IRequest<VRedTransactionResponse>
    {
        public CreateTransactionRequest Request { get; set; }
        public string RequestId { get; set; }
    }
}