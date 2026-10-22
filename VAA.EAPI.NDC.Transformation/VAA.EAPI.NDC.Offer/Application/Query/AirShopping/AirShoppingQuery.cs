using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRS;

namespace VAA.EAPI.NDC.Offer.Application.Query.AirShopping
{
    public class AirShoppingQuery : IRequest<Result<Response<IATA_AirShoppingRS>>>
    {
        public IATA_AirShoppingRQ AirShoppingRQ { get; set; }
        public string SubscriptionKey { get; set; }

        public AirShoppingQuery(IATA_AirShoppingRQ airShoppingRQ, string subscriptionKey)
        {
            AirShoppingRQ = airShoppingRQ;
            SubscriptionKey = subscriptionKey;
        }
    }
}