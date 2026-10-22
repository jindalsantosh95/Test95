using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IAutoAccrualFeedbackPublisher
    {
        Task PublishSuccessAsync(string correlationId, string messageId, TransactionDocument transaction, CancellationToken cancellationToken);
        Task PublishErrorAsync(string correlationId, string messageId, string errorCode, string errorDescription, CancellationToken cancellationToken);
    }
}
