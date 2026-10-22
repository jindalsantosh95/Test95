using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;


namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentStatusEventCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
