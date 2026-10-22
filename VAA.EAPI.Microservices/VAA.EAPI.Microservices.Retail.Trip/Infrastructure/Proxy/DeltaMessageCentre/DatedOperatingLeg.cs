namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    public class DatedOperatingLeg
    {

        public int? LegId { get; set; }

        public int? Ordinal { get; set; }

        public IEnumerable<Carrier> Carrier { get; set; }

        public string FlightNum { get; set; }

        public TransportOrigin TransportOrigin { get; set; }

        public TransportDestination TransportDestination { get; set; }
    }
}
