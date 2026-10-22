using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class CarrierDetails
    {
        public string FlightNumber { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }
        public string OperationalSuffix { get; set; }

        public static explicit operator CarrierDetails(MarketingSegment source)
        {
            if (source == null)
                return null;

            return new CarrierDetails()
            {
                CarrierCode = source.Carrier?.FirstOrDefault()?.Code,
                CarrierName = source.Carrier?.FirstOrDefault()?.Name,
                FlightNumber = source.FlightNum,
                OperationalSuffix = source.OperationalSuffix
            };
        }
    }
}
