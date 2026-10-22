using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services
{
    public interface IAirshipService
    {
        Task Handle(AirshipRequest request, CancellationToken cancellationToken);
    }
}
