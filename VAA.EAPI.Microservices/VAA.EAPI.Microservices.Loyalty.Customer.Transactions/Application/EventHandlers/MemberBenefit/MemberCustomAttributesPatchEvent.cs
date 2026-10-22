using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MemberBenefit
{
    public class MemberCustomAttributesPatchEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public MemberCustomAttributesPatchEvent()
        {
            EventType = CosmosModificationType.Replace;
        }
    }
}
