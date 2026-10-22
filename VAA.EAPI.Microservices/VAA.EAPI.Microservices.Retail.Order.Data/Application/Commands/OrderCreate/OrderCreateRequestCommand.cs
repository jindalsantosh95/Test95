using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCreate
{
    public class OrderCreateRequestCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
}

