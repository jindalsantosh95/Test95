using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.GroupDoc
{
    public class GroupDocPatchEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public GroupDocPatchEvent()
        {
            EventType = CosmosModificationType.Replace;
        }
    }
}
