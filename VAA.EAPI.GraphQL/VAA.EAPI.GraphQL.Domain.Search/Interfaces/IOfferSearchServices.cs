using VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo.FareRules;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface IOfferSearchServices
    {
        Task<FlightOfferResponse> FlightOfferSearchAsync(FlightOfferRequest request);

        Task<FlexiDateSearchResponse?> FlexiDateSearchAsync(FlexiDateOfferRequest request);

        Task<CalendarSearchResponse?> CalendarSearchAsync(CalendarOfferRequest request);

        Task<OfferFareRules> FareRulesAsync(string offerId);
    }
}