using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCancelUpdate
{
    public class CancelOrderUpdateCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
