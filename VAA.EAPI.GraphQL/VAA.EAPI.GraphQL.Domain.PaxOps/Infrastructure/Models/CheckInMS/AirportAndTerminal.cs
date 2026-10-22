namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class AirportAndTerminal
    {
        public string TerminalName { get; set; }
        public string Code { get; set; }
        public string AirportName { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }

        public static explicit operator AirportAndTerminal(VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips.AirportDetails airportDetails)
        {
            return new AirportAndTerminal
            {
                TerminalName = airportDetails?.TerminalName,
                Code = airportDetails?.Code,
                AirportName = airportDetails?.AirportName,
                CityName = airportDetails?.CityName,
                CountryCode = airportDetails?.CountryCode,
                CountryName = airportDetails?.CountryName
            };
        }
    }
}
