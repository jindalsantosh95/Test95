namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    public class DatedMarketingSegment
    {
        public string FlightNumber { get; set; }

        public IEnumerable<Carrier> MarketingCarrier { get; set; }
    }
}
