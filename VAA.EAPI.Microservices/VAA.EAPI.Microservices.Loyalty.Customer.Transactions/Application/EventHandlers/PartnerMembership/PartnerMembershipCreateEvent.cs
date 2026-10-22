using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.PartnerMembership
{
    public class PartnerMembershipCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public PartnerMembershipCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }

    }

}
