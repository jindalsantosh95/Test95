using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public interface IStarlinkNotificationEventCommand
    {
        public Task Handle(CloudEvent cloudEventReq, CancellationToken cancellationToken);
    }
}
