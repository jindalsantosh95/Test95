using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.InviteHouseHold
{
    public class InviteHouseHoldDeleteEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public InviteHouseHoldDeleteEvent()
        {
            EventType = CosmosModificationType.Delete;
        }
    }
}
