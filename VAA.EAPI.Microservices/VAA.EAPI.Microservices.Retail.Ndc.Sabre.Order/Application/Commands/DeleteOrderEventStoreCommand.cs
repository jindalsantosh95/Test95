using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands
{
    public class DeleteOrderEventStoreCommand : CloudEvent,IRequest<Result<Unit>>
    {
    }
}
