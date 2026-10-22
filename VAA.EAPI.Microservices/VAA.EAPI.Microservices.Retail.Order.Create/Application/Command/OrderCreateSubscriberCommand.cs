using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Application.Command
{
    public class OrderCreateSubscriberCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
}
