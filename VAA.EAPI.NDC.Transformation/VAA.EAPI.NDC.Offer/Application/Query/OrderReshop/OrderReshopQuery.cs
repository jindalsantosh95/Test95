using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;

namespace VAA.EAPI.NDC.Offer.Application.Query.OrderReshop
{
    public class OrderReshopQuery : IRequest<Result<Response<IATA_OrderReshopRS>>>
    {
        public IATA_OrderReshopRQ OrderReshopRQ { get; set; }
        public string SubscriptionKey { get; set; }

        public OrderReshopQuery(IATA_OrderReshopRQ orderReshopRQ, string subscriptionKey)
        {
            OrderReshopRQ = orderReshopRQ;
            SubscriptionKey = subscriptionKey;
        }
    }
}
