using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class AirlineDetails
    {
        public string FlightNumber { get; set; }
        public string CarrierCode { get; set; }
        public string Carriername { get; set; }

        public static explicit operator AirlineDetails(OperatingCarrier? source)
        {
            if (source == null)
                return null;

            return new AirlineDetails()
            {
                CarrierCode = source.Code,
                Carriername = source.Name
            };
        }

        public static explicit operator AirlineDetails(MarketingSegment source)
        {
            if (source == null)
                return null;

            return new AirlineDetails()
            {
                CarrierCode = source.OperationalSuffix,
                Carriername = source.Carrier?.FirstOrDefault()?.Name,
                FlightNumber = source.FlightNum
            };
        }

        public static explicit operator AirlineDetails(Carrier? source)
        {
            if (source == null)
                return null;

            return new AirlineDetails()
            {
                CarrierCode = source.Code,
                Carriername = source.Name
            };
        }
    }
}
