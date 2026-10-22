using System;

namespace VAA.EAPI.Orchestration.Notifications.Domain.Entities
{
    public class Flight
    {
        public string Departure{ get; set; }
        public string Arrival{ get; set; }
        public Carrier OperatingCarrier { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public string NotificationType { get; set; } = Constants.Constants.NotificationTypeCheckInOpen;
    }
}
