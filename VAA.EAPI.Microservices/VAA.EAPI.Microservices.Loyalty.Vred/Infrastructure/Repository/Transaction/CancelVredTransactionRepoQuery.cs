using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction
{
    public class CancelVredTransactionRepoQuery : IRequest<UpdateTransactionResponse>
    {
        public CancelVredTransactionRequestData CancelVredTransactionData { get; set; }

        public CancelVredTransactionRepoQuery(CancelVredTransactionRequestData cancelVredTransactionData)
        {
            CancelVredTransactionData = cancelVredTransactionData;
        }
    }
}
