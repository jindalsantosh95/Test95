using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS;

namespace VAA.EAPI.NDC.Offer.Application.Query.OfferPrice
{
    public class OfferPriceQuery : IRequest<Result<Response<IATA_OfferPriceRS>>>
    {
        public IATA_OfferPriceRQ OfferPriceRQ { get; set; }

        public string SubscriptionKey { get; set; }

        public OfferPriceQuery(IATA_OfferPriceRQ offerPriceRQ, string subscriptionKey)
        {
            OfferPriceRQ = offerPriceRQ;
            SubscriptionKey = subscriptionKey;
        }
    }
}