using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation
{
    public class OrderAssociationEventCommand : IRequest<Result<Response<string>>>
    {
        public CloudEvent cloudEvent { get; set; }
    }

}
