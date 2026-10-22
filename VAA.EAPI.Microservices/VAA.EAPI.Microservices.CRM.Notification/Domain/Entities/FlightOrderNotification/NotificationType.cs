namespace VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification
{
    public enum NotificationType
    {
        FlightCancelled,
        FlightDelayed,
        FlightTimeChanged,
        FlightDiverted,
        FlightAircraftChanged,
        GateAssigned,
        BoardingStarted,
        BoardingCompleted,
        CheckInOpened,
        CheckInClosed,
    }
}
