using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop;

namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public interface IIropEventCommand
    {
        Task Handle(Ctx bizTalkmessage, CancellationToken cancellationToken);
    }
}