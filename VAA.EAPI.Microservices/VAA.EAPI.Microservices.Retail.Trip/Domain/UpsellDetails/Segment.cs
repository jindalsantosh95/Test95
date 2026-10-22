using DLSchema = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class Segment
    {
        public string Id { get; set; }
        public CarrierDetails? OperatingCarrier { get; set; }
        public CarrierDetails? MarketingCarrier { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }
        public IEnumerable<Leg>? Legs { get; set; }
        public AirlineDetails? Operating { get; set; }
        public AirlineDetails? Marketing { get; set; }

        public IEnumerable<UpsellOffer> UpsellOffers { get; set; }


        public static explicit operator Segment(DLSchema.UpsellSegment source)
        {
            if (source == null) return null;

            return new Segment
            {
                Id = source.DatedOperatingSegment?.SegmentId.ToString(),
                OperatingCarrier = (CarrierDetails)(source.DatedOperatingSegment?.Carrier, source.DatedOperatingSegment?.FlightNum),
                MarketingCarrier = (CarrierDetails)source.DatedOperatingSegment?.MarketingSegment,
                Origin = (AirportDetails)source.DatedOperatingSegment?.OriginStation,
                Destination = (AirportDetails)source.DatedOperatingSegment?.DestinationStation,
                Legs = source.DatedOperatingSegment?.Legs?.Select(leg => (Leg)leg),
                Operating = (AirlineDetails)(source.DatedOperatingSegment?.FlightNum, source.DatedOperatingSegment?.Carrier),
                Marketing = (AirlineDetails)source.DatedOperatingSegment?.MarketingSegment,
                UpsellOffers = source.UpsellOffers.Select(uo => (UpsellOffer)uo)
            };
        }
    }
}
