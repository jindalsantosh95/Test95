namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{

    public class DatedOperatingSegment
    {

        public int? SegmentId { get; set; }

        public int? Ordinal { get; set; }

        public IEnumerable<Carrier> OperatingCarrier { get; set; }

        public string FlightNum { get; set; }

        public DatedMarketingSegment MarketingSegment { get; set; }

        public Station OriginStation { get; set; }

        public Station DestinationStation { get; set; }

        public IEnumerable<DatedOperatingLeg> Legs { get; set; }
    }
}
