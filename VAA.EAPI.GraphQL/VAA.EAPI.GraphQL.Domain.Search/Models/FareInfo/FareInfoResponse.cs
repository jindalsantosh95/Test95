using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo.FareRules;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo
{
    public class FareInfoResponse
    {
        public IEnumerable<FareRule> Rules { get; set; }

        public FareInfoResponse(FlightOfferResponse offerResponse, OfferFareRules rules, IEnumerable<AirportInfo> airportDetails)
        {
            Rules = rules.FareRules.Select(rule =>
            {
                var segments = offerResponse.DataList.FlightSegments.Where(x => x.OriginCityCode == rule.OriginAirportCode && x.DestinationCityCode == rule.DestinationAirportCode);
                return new FareRule()
                {
                    FareBasisCode = rule.FareBasisCode,
                    Detail = (Detail)rule.OfferdFareRuleCategoryList.ToArray(),
                    Segments = segments.Select(segment => new FareRuleSegment(segment, airportDetails))
                };
            });
        }
    }
}