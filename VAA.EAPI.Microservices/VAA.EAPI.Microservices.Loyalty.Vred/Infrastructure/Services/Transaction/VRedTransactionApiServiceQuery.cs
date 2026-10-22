using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VRedTransactionApiServiceQuery : IRequest<VRedTransactionResponse>
    {
        public TransactionPostAPI TransactionPostRequest { get; set; }
    }
}