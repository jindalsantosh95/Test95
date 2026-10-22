using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction
{
    public class TransactionCancelEvent : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
