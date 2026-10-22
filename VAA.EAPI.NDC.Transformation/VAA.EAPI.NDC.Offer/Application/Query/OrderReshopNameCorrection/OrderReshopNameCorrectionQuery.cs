using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;

namespace VAA.EAPI.NDC.Offer.Application.Query.OrderReshopNameCorrection
{
    public class OrderReshopNameCorrectionQuery : IRequest<Result<Response<IATA_OrderReshopRS>>>
    {
        public IATA_OrderReshopRQ OrderReshopRQ { get; set; }
        public string SubscriptionKey { get; set; }

        public OrderReshopNameCorrectionQuery(IATA_OrderReshopRQ orderReshopRQ, string subscriptionKey)
        {
            OrderReshopRQ = orderReshopRQ;
            SubscriptionKey = subscriptionKey;
        }
    }
}
