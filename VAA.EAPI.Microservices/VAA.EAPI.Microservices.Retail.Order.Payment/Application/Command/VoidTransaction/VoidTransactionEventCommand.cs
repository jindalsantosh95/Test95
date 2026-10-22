using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.VoidTransaction
{
    public class VoidTransactionEventCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
