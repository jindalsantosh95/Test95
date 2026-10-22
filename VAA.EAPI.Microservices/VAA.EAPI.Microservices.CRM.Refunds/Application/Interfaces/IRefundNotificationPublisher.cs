using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;

public interface IRefundNotificationPublisher
{
    Task HandleAsync(RefundRequestForm request, string claimNumber, CancellationToken cancellationToken);
}
