using System; 

namespace VAA.EAPI.Orchestration.Notifications.Domain.Entities
{ 
    public class Leg
    {
        public Carrier OperatingCarrier { get; set; }
        public Carrier MarketingCarrier { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledDepartureUtcDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public DateTime ScheduledArrivalUtcDateTime { get; set; }
        public Airport Origin { get; set; }
        public Airport Destination { get; set; }
    }

}