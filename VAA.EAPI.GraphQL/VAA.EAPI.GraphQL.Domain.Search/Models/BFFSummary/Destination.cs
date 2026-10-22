using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    [GraphQLName("Destination_BFF")]
    public class Destination
    {
        public string? Code { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? AirportName { get; set; }

        public static explicit operator Destination((string airportCode, IEnumerable<AirportInfo> airportInfo) source)
        {
            var airport = source.airportInfo.FirstOrDefault(ai => ai.AirportCode == source.airportCode);

            return new()
            {
                Code = airport?.AirportCode ?? source.airportCode,
                CityName = airport?.CityName,
                CountryName = airport?.CountryName,
                AirportName = airport?.AirportName ?? source.airportCode
            };
        }
    }
}