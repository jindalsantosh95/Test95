
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class DatedMarketingSegment
    {
        public string FlightNumber { get; set; }

        public IEnumerable<Carrier> MarketingCarrier { get; set; }

        public static explicit operator DatedMarketingSegment(Infrastructure.Proxy.DeltaMessageCentre.DatedMarketingSegment source)
        {
            if (source == null)
                return null;

            return new DatedMarketingSegment()
            {
                FlightNumber = source.FlightNumber,
                MarketingCarrier = source.MarketingCarrier?.Select(carrier => (Carrier)carrier)
            };
        }
    }
}
