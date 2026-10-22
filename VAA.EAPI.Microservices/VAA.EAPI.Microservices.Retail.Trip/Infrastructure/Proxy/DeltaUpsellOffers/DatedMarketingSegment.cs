namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class DatedMarketingSegment
    {
        public string FlightNum { get; set; }
        public IEnumerable<Carrier> Carrier { get; set; }
    }
}
