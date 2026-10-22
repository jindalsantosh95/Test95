namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class TransportDestination
    {
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public DateTime EstimatedArrivalLocalDateTime { get; set; }
        public Station Station { get; set; }
    }
}
