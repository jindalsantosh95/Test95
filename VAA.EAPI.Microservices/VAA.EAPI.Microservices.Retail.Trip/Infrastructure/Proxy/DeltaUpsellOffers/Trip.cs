namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class Trip
    {
        public int TripId { get; set; }
        public int Ordinal { get; set; }
        public TransportOrigin TransportOrigin { get; set; }
        public TransportDestination TransportDestination { get; set; }
    }
}
