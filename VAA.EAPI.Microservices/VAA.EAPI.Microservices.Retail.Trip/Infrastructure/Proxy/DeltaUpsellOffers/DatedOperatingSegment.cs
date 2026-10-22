namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class DatedOperatingSegment
    {
        public int SegmentId { get; set; }
        public int Ordinal { get; set; }
        public string FlightNum { get; set; }
        public Carrier Carrier { get; set; }
        public DatedMarketingSegment MarketingSegment { get; set; }
        public Station OriginStation { get; set; }
        public Station DestinationStation { get; set; }
        public IEnumerable<DatedOperatingLeg> Legs { get; set; }
    }
}
