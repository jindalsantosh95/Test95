using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsPos")]
    public class Pos
    {
        public string? CountryCode { get; set; }
        public string? CityCode { get; set; }
        public string? CurrencyCode { get; set; }

        public static explicit operator Pos((Infrastructure.Models.TripMS.Booking trip, IEnumerable<AirportRefData> airportRefDatas) source)
        {
            if (source.airportRefDatas == null || source.trip == null)
                return null;

            var (trip, airportRefDatas) = source;

            return new Pos()
            {
                CountryCode = trip.Pos.CountryCode,
                CityCode = airportRefDatas.FirstOrDefault(city => city.AirportCode == trip.Product?.FirstOrDefault(p => p.BookingType == "AIRLINE")?.Slices?.FirstOrDefault()?.Segments?.FirstOrDefault()?.Origin.Code)?.CityCode,
                CurrencyCode = trip.Pos.CurrencyCode ?? Constants.Application.DefaultCurrencyCode
            };
        }

        public static explicit operator Pos(Infrastructure.Models.TripMS.Pos pos)
        {
            return new Pos()
            {
                CountryCode = pos.CountryCode,
                CityCode = pos.CountryCode,
            };
        }
    }
}
