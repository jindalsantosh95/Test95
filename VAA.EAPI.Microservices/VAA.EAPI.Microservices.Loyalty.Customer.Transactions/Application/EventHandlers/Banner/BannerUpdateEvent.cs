using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Banner
{
    public class BannerUpdateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public BannerUpdateEvent()
        {
             EventType = CosmosModificationType.Replace;
        }
    }
}
