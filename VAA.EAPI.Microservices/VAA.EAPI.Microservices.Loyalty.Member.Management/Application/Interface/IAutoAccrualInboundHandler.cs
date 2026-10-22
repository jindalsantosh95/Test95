using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IAutoAccrualInboundHandler
    {
        Task<TransactionDocument> HandleAsync(string eventRequestPayload, CancellationToken cancellationToken);
    }
}
