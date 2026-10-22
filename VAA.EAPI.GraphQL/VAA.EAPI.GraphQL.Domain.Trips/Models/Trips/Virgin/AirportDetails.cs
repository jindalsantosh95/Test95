using HotChocolate;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsAirportDetails")]
    public class AirportDetails
    {
        public string? TerminalName { get; set; }
        public string? Code { get; set; }
        public string? AirportName { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }

        public static explicit operator AirportDetails(Infrastructure.Models.TripMS.AirportDetails source)
        {
            if (source == null)
                return null;

            return new AirportDetails()
            {
                AirportName = source.AirportName,
                CityName = source.CityName,
                Code = source.Code,
                CountryCode = source.CountryCode,
                CountryName = source.CountryName,
                TerminalName = source.TerminalName,
            };
        }

        public static explicit operator AirportDetails(Infrastructure.Models.HolidaysMS.TripsCore.AirportDetails source)
        {
            if (source == null)
                return null;

            return new AirportDetails()
            {
                AirportName = source.AirportName,
                CityName = source.CityName,
                Code = source.Code,
                CountryCode = source.CountryCode,
                CountryName = source.CountryName,
                TerminalName = source.TerminalName,
            };
        }

        public static explicit operator AirportDetails((Infrastructure.Models.HolidaysMS.TripsCore.AirportDetails airportDetails, IEnumerable<AirportRefData> refAirportData) source)
        {
            var (airportDetails, refAirportData) = source;

            if (airportDetails == null)
                return null;

            var airportData = refAirportData?.FirstOrDefault(x => x.AirportCode.Is(airportDetails.Code));

            return new AirportDetails()
            {
                AirportName = airportData?.AirportName ?? airportDetails.AirportName,
                CityName = airportData?.CityName ?? airportDetails.CityName,
                Code = airportDetails.Code,
                CountryCode = airportData?.CountryCode ?? airportDetails.CountryCode,
                CountryName = airportData?.CountryName ?? airportDetails.CountryName,
                TerminalName = airportDetails?.TerminalName,
            };
        }
    }
}
