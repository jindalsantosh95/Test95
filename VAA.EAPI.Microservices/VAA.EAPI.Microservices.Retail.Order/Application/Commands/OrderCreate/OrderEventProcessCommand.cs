using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate
{
    public class OrderEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
}
