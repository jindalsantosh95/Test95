using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward
{
    public class CustomerAssignAwardCommand : IRequest<Result<Response<string>>>
    {
        public CloudEvent CloudEvent { get; set; }
    }
}
