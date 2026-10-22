namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    public class TransportDestination
    {
        public string ScheduledArrivalLocalDateTime { get; set; }
        public string EstimatedArrivalLocalDateTime { get; set; }
        public Station Station { get; set; }
    }
}
