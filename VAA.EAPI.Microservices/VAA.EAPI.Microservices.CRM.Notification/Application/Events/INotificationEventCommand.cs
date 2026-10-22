using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Notification.Application.Events
{
    public interface INotificationEventCommand
    {
        Task Handle(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }
}