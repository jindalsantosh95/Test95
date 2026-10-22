using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Repositories
{
    public interface IAepApiRepoQuery
    {
        Task Handle(List<FlightOrderData> notificationRequest, CancellationToken cancellationToken);
    }
}
