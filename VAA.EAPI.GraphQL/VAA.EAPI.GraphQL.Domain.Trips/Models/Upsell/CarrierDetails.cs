using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellCarrierDetails")]
    public class CarrierDetails
    {
        public string FlightNumber { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }

        public static explicit operator CarrierDetails((Infrastructure.Models.UpsellOffer.CarrierDetails Operating, IEnumerable<AirlineRefData> airlineRefData) data)
        {
            var (source, airlineRefData) = data;
            if (source == null)
                return null;

            return new CarrierDetails
            {
                CarrierCode = source.CarrierCode,
                CarrierName = airlineRefData?.FirstOrDefault(a => a.CarrierCode == source.CarrierCode)?.CarrierName ?? source.CarrierName,
                FlightNumber = source.FlightNumber,
            };
        }
    }
}
