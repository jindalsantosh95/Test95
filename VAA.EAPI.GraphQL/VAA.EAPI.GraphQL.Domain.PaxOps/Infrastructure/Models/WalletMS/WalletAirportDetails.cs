using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS
{
    public class WalletAirportDetails
    {
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string TimeZone { get; set; }

        public static explicit operator WalletAirportDetails(AirportDetails airportDetails)
        {
            return new WalletAirportDetails
            {
                AirportCode = airportDetails?.AirportCode,
                AirportName = airportDetails?.AirportName,
                CityCode = airportDetails?.CityCode,
                CityName = airportDetails?.CityName,
                CountryCode = airportDetails?.CountryCode,
                CountryName = airportDetails?.CountryName,
                TimeZone = airportDetails?.TimeZone
            };
        }
    }
}
