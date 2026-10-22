
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
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

        public static explicit operator DatedOperatingSegment(Infrastructure.Proxy.DeltaMessageCentre.DatedOperatingSegment source)
        {
            if (source == null)
                return null;

            return new DatedOperatingSegment()
            {
                DestinationStation = (Station)source.DestinationStation,
                FlightNum = source.FlightNum,
                MarketingSegment = (DatedMarketingSegment)source.MarketingSegment,
                OriginStation = (Station)source.OriginStation,
                OperatingCarrier = source.OperatingCarrier?.Select(carrier => (Carrier)carrier),
                SegmentId = source.SegmentId,
                Ordinal = source.Ordinal,
                Legs = source.Legs?.Select(leg => (DatedOperatingLeg)leg)

            };
        }
    }
}
