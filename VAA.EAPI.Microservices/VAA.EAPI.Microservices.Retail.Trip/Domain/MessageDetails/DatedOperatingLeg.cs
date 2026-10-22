
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class DatedOperatingLeg
    {

        public int? LegId { get; set; }

        public int? Ordinal { get; set; }

        public IEnumerable<Carrier> Carrier { get; set; }

        public string FlightNum { get; set; }

        public TransportOrigin TransportOrigin { get; set; }

        public TransportDestination TransportDestination { get; set; }

        public static explicit operator DatedOperatingLeg(Infrastructure.Proxy.DeltaMessageCentre.DatedOperatingLeg source)
        {
            if (source == null)
                return null;

            return new DatedOperatingLeg()
            {
                Carrier = source.Carrier?.Select(carrier => (Carrier)carrier),
                FlightNum = source.FlightNum,
                LegId = source.LegId,
                Ordinal = source.Ordinal,
                TransportOrigin = (TransportOrigin)source.TransportOrigin,
                TransportDestination = (TransportDestination)source.TransportDestination
            };
        }
    }
}
