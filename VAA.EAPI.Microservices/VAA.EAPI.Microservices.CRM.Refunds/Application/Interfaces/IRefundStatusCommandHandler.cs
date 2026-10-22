using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;

public interface IRefundStatusCommandHandler
{
    Task<RefundRequestForm> HandleAsync(RefundRequestForm request, CancellationToken cancellationToken);
}
