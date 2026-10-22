using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces
{
    public interface IOtpEmailEventPublisher
    {
        Task Publish(CloudEvent emailEvent, string correlationIdForEvents, CancellationToken cancellationToken);
    }
}
