using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.PaymentUpdate
{
    public class PaymentUpdateCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
