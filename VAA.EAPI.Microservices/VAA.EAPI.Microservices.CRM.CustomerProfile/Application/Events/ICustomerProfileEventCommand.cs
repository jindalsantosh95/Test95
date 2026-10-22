using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Events
{
    public interface ICustomerProfileEventCommand
    {
        Task Handle(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }
}