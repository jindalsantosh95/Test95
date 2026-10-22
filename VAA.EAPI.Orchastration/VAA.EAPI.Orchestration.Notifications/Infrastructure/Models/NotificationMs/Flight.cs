using System;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs
{
    public class Flight
    {
        public static explicit operator Flight(Domain.Entities.Flight source) => new()
        {
            OperatingCarrier = (Carrier)source.OperatingCarrier,
            ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
            NotificationType = source.NotificationType
        };

        public Carrier OperatingCarrier { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public string NotificationType { get; set; }
    }
}
