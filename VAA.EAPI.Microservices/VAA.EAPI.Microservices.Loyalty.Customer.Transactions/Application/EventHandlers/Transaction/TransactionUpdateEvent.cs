using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction
{
    public class TransactionUpdateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public TransactionUpdateEvent()
        {
            EventType = CosmosModificationType.Replace;
        }
    }
}
