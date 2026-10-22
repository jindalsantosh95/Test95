using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MemberBenefit
{
    public interface IMemberCustomAttributesCreateEventHandler
    {
        Task<Result<Unit>> Handle(MemberCustomAttributesCreateEvent memberCustomAttributesCreateEvent, CancellationToken cancellationToken);
    }
}
