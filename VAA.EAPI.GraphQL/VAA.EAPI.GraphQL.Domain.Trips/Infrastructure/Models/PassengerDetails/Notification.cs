namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class Notification
    {
        public NotificationActionType Action { get; set; }
        public Error Content { get; set; }
    }
}