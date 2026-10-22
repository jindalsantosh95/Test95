using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellAirportDetails")]
    public class AirportDetails
    {
        public string? Code { get; set; }
        public string? AirportName { get; set; }

        public static explicit operator AirportDetails((Infrastructure.Models.UpsellOffer.AirportDetails Origin, IEnumerable<AirportRefData> airportRefdata) data)
        {
            var (source, airportRefdata) = data;
            if (source == null) return null;

            return new AirportDetails()
            {
                Code = source.Code,
                AirportName = airportRefdata?.FirstOrDefault(a => a.AirportCode == source.Code)?.AirportName ?? source.AirportName
            };
        }
    }
}
