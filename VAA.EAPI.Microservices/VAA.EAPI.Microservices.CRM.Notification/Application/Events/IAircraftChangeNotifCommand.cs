using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Notification.Application.Events
{
    public interface IAircraftChangeNotifCommand
    {
        Task Handle(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }
}