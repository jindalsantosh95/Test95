
using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    [GraphQLName("GraphqlSSRAirport")]
    public class Airport
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? CityCode { get; set; }
        public string? CityName { get; set; }

        public static explicit operator Airport((Infrastructure.Models.SSRMS.Airport airport, IEnumerable<AirportRefData> airportRefData) source)
        {
            if (source.airport == null)
                return null;

            var airportData = source.airportRefData?.FirstOrDefault(a => a.AirportCode == source.airport.Code);

            return new()
            {
                Name = source.airport.Name,
                Code = source.airport.Code,
                CityCode = airportData?.CityCode,
                CityName = airportData?.CityName
            };
        }
    }
}