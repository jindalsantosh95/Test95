using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class FlightSearchResponse
    {
        public FlightOfferResponse? FlightOfferResponse { get; set; }
        public FlexiDateSearchResponse? FlexiDateSearchResponse { get; set; }
        public CalendarSearchResponse? CalendarSearchResponse { get; set; }
    }
}