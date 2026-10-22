using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellAirlineDetails")]
    public class AirlineDetails
    {
        public string FlightNumber { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }

        public static explicit operator AirlineDetails((Infrastructure.Models.UpsellOffer.AirlineDetails Operating, IEnumerable<AirlineRefData> airlineRefData) data)
        {
            var (Operating, airlineRefData) = data;
            if (Operating == null) return null;

            return new AirlineDetails()
            {
                FlightNumber = Operating.FlightNumber,
                CarrierCode = Operating.CarrierCode,
                CarrierName = airlineRefData?.FirstOrDefault(a => a.CarrierCode == Operating.CarrierCode)?.CarrierName ?? Operating.CarrierName
            };
        }
    }
}
