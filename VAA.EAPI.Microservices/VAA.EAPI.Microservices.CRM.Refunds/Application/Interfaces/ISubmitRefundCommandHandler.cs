using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;

public interface ISubmitRefundCommandHandler
{
    Task<RefundSubmissionResponse> HandleAsync(RefundRequestForm request, CancellationToken cancellationToken);
}
