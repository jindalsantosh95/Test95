using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events
{
    public interface ISplitPnrEventProcessCommand 
    {
        Task<Result<Unit>> Handle(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }
}
