using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction
{
    public interface IUpdateVredTransactionServiceHandler
    {
        Task<UpdateTransactionResponse> Handle(UpdateVredTransactionService request, CancellationToken cancellationToken);
    }
}
