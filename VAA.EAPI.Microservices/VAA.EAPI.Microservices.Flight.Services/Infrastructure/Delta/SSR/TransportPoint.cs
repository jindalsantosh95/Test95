namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class TransportPoint
    {
        public string ScheduledDepartureLocalDateTime { get; set; }
        public string ScheduledArrivalLocalDateTime { get; set; }
        public Station Station { get; set; }
    }
}
