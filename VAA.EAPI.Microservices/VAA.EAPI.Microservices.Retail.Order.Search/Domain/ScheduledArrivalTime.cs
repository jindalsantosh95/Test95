namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class ScheduledArrivalTime
    {
        public string UtcDateTime { get; set; }
        public string LocalDateTime { get; set; }
    }
    public class ScheduledDepartureTime
    {
        public string UtcDateTime { get; set; }
        public string LocalDateTime { get; set; }
    }
}
