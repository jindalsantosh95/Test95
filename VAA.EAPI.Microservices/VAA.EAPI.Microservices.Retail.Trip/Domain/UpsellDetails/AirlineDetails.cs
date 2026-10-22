using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class AirlineDetails
    {
        public string FlightNumber { get; set; }
        public string CarrierCode { get; set; }
        public string Carriername { get; set; }

        public static explicit operator AirlineDetails(DatedMarketingSegment? source)
        {
            if (source == null) return null;

            return new AirlineDetails
            {
                FlightNumber = source.FlightNum,
                CarrierCode = source.Carrier?.FirstOrDefault()?.Code,
                Carriername = source.Carrier?.FirstOrDefault()?.Name
            };
        }

        public static explicit operator AirlineDetails((string? FlightNum, Carrier? Carrier) source)
        {
            var (flightNum, carrier) = source;

            if (carrier == null && flightNum == null) return null;

            return new AirlineDetails
            {
                FlightNumber = flightNum,
                CarrierCode = carrier?.Code,
                Carriername = carrier?.Name
            };
        }

        public static explicit operator AirlineDetails((string? FlightNum, Infrastructure.Proxy.DeltaUpsellOffers.Carrier? Carrier) source)
        {
            var (flightNum, carrier) = source;
            if (carrier == null && flightNum == null) return null;

            return new AirlineDetails
            {
                FlightNumber = flightNum,
                CarrierCode = carrier?.Code,
                Carriername = carrier?.Name
            };
        }
    }
}
