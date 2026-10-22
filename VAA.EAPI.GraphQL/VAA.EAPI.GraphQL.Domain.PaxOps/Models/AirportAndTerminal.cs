using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:AirportAndTerminal")]
    public class AirportAndTerminal
    {
        [GraphQLDescription("Terminal name or number.")]
        public string TerminalName { get; set; }

        [GraphQLDescription("Airport IATA code.")]
        public string Code { get; set; }

        [GraphQLDescription("Airport display name.")]
        public string AirportName { get; set; }

        [GraphQLDescription("Destination city display name, when available from Reference MS airport data.")]
        public string CityName { get; set; }

        [GraphQLDescription("Country display name.")]
        public string CountryName { get; set; }

        [GraphQLDescription("Airport country code. Uses ISO alpha-3 codes, with the Germany exception D.")]
        public string CountryCode { get; set; }

        public static explicit operator AirportAndTerminal(Infrastructure.Models.CheckInMS.AirportAndTerminal source) => new()
        {
            TerminalName  = source.TerminalName,
            Code  = source.Code,
            AirportName  = source.AirportName,
            CityName = source.CityName,
            CountryCode  = source.CountryCode,
            CountryName = source?.CountryName
        };
    }
}
