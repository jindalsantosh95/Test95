namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class TransportOrigin
    {
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime EstimatedDepartureLocalDateTime { get; set; }
        public Station Station { get; set; }
    }
}
