using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.GroupDoc
{
    public class GroupDocCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public GroupDocCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
}