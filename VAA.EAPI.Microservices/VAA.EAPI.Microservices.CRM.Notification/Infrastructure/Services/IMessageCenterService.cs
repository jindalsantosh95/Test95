using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.AircraftChangeNotification;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services
{
    public interface IMessageCenterService
    {
        Task Handle(AircraftChangeRequest request, CancellationToken cancellationToken);
    }
}
