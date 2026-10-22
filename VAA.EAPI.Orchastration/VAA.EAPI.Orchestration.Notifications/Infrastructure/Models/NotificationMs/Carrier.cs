namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs
{
    public class Carrier
    {
        public static explicit operator Carrier(Domain.Entities.Carrier source) => new()
        {
            FlightNum = source.FlightNum
        };

        public string FlightNum { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }
    }
}
