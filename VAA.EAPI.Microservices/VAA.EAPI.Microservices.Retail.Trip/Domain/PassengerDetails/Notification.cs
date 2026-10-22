using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class Notification
    {
        public NotificationActionType Action { get; set; }
        public Error Content { get; set; }

        public static explicit operator Notification(DeltaPaxInfo.Notification source)
        {
            if (source is null) return null;

            return new Notification()
            {
                Action = (NotificationActionType)source.Action,
                Content = (Error)source.Content
            };
        }
    }
}