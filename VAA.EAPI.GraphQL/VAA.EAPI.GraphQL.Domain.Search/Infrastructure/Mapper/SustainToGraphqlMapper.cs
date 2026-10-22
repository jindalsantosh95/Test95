using System.Net;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Search.Models.Sustain;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper
{
    public class SustainToGraphqlMapper
    {
        private static readonly Dictionary<string, string[]> fareFamily = new Dictionary<string, string[]>
        {
            { "Economy", new string[] { "ECONOMY-LIGHT", "ECON", "ECON-CLASSIC-FLEXIBLE", "ECONOMY-DELIGHT", "ECON-DELIGHT-FLEXIBLE" } },
            { "Premium", new string[] {  "COMFORT-PLUS-PREMIUM-ECON", "COMFORT-PLUS-PREMIUM-FLEXIBLE"} },
            { "Business", new string[] { "BUS-FIRST", "BUS-FIRST-FLEXIBLE" } }
        };

        private static readonly Dictionary<string, string[]> awardFareFamily = new Dictionary<string, string[]>
        {
            { "Economy", new string[] { "AWARD-ECONOMY" } },
            { "Premium", new string[] { "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY" } },
            { "Business", new string[] { "AWARD-BUSINESS-FIRST" } }
        };

        public static IEnumerable<CarbonIndexResponse> MaptoGraphqlResponse(IEnumerable<IEnumerable<SustainMSResponse>> sustainMSResponses, int passengerCountWithoutInfant, bool isAwardSearch)
        {
            return sustainMSResponses.SelectMany(sustainMSResponse => sustainMSResponse
                                       .GroupBy(s => $"{s.OriginAirport}-{s.DestinationAirport}-{s.Cabin}")
                                        .Where(data => data != null && data.Count() == 2)
                                        .Select(data => MapTotalPTCCarbonData(data, passengerCountWithoutInfant, isAwardSearch)));

        }

        private static CarbonIndexResponse MapTotalPTCCarbonData(IGrouping<string, SustainMSResponse> data, int passengerCountWithoutInfant, bool isAwardSearch)
        {
            var ptcCount = passengerCountWithoutInfant == 0 ? 1 : passengerCountWithoutInfant;

            var actualCo2eMSResponse = data.First(d => !d.FlightNumber.IsNullOrEmpty());
            var refCo2eMSResponse = data.First(d => d.FlightNumber.IsNullOrEmpty());
            var farefamilylist = isAwardSearch ? awardFareFamily : fareFamily;

            if (data.Any(d => d.StatusCode != ((int)HttpStatusCode.OK).ToString()))
                return new()
                {
                    OriginAirport = actualCo2eMSResponse.OriginAirport,
                    DestinationAirport = actualCo2eMSResponse.DestinationAirport,
                    FlightNumber = actualCo2eMSResponse.FlightNumber,
                    FareFamilies = farefamilylist.First(f => f.Key == actualCo2eMSResponse.Cabin).Value
                };

            return new()
            {
                DestinationAirport = actualCo2eMSResponse.DestinationAirport,
                OriginAirport = actualCo2eMSResponse.OriginAirport,
                KilosCo2e = actualCo2eMSResponse.KilosCo2e * ptcCount,
                FlightNumber = actualCo2eMSResponse.FlightNumber,
                FlightDate = actualCo2eMSResponse.FlightDate,
                Source = actualCo2eMSResponse.Source,
                SourceMethodology = actualCo2eMSResponse.SourceMethodology,
                SourceQuality = actualCo2eMSResponse.SourceQuality,
                RefKilosCo2e = refCo2eMSResponse.KilosCo2e * ptcCount,
                RefSource = refCo2eMSResponse.Source,
                RefSourceMethodology = refCo2eMSResponse.SourceMethodology,
                RefSourceQuality = refCo2eMSResponse.SourceQuality,
                FareFamilies = farefamilylist.First(f => f.Key == actualCo2eMSResponse.Cabin).Value

            };
        }
    }
}
