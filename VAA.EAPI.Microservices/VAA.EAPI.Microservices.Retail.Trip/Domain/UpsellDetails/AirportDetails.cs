using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class AirportDetails
    {
        public string? Code { get; set; }
        public string? AirportName { get; set; }
     
        public static explicit operator AirportDetails(TransportOrigin? source)
        {
            if (source == null)
                return null;

            return new AirportDetails
            {
                Code = source?.Station?.Code,
                AirportName = source?.Station?.Name
            };
        }

        public static explicit operator AirportDetails(TransportDestination? source)
        {
            if (source == null)
                return null;

            return new AirportDetails
            {
                Code = source?.Station?.Code,
                AirportName = source?.Station?.Name
            };
        }

        public static explicit operator AirportDetails(Station? source)
        {
            if (source == null) return null;

            return new AirportDetails
            {
                Code = source?.Code,
                AirportName = source?.Name
            };
        }
    }
}
