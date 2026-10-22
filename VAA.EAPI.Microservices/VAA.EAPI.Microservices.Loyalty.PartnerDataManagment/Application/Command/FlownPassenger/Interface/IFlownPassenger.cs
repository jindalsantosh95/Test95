
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger.Interface
{
    public interface IFlownPassenger
    {
        Task<Response<string>> Handle(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }
}
