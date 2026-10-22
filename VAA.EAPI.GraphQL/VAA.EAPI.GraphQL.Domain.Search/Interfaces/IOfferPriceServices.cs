using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface IOfferPriceServices
    {
        Task<FlightOfferResponse> OfferPriceAsync(FlightOfferPriceRequest flightOfferRequest);

        Task<FlightOfferResponse> OfferPriceDeeplinkAsync(FlightDeepLinkSummaryRequest deepLinkSummaryRequest);
    }
}
