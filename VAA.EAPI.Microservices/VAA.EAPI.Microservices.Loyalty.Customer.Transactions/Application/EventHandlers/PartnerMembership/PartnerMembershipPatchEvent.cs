using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.PartnerMembership
{
    public class PartnerMembershipPatchEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public PartnerMembershipPatchEvent()
        {
            EventType = CosmosModificationType.Replace;
        }

    }
}
