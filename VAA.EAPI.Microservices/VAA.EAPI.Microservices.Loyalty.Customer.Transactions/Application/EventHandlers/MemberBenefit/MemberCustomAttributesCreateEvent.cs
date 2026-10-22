using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MemberBenefit
{
    public class MemberCustomAttributesCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public MemberCustomAttributesCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }

}
