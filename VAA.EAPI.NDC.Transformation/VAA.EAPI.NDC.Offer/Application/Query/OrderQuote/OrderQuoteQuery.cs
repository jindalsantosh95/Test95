using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;

namespace VAA.EAPI.NDC.Offer.Application.Query.OrderQuote
{
    public class OrderQuoteQuery : IRequest<Result<Response<IATA_OrderReshopRS>>>
    {
        public IATA_OrderQuoteRQ OrderQuoteRQ { get; set; }
        public string SubscriptionKey { get; set; }

        public OrderQuoteQuery(IATA_OrderQuoteRQ orderQuoteRQ, string subscriptionKey)
        {
            OrderQuoteRQ = orderQuoteRQ;
            SubscriptionKey = subscriptionKey;
        }
    }
}
