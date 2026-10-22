using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    
    public class DeleteOrderChangeEventStoreCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
