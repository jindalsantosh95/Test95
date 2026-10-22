using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public interface IAcSwapsFlightDataRequestEventCommand
    {
        Task Handle(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }
}