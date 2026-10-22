using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NotificationActionType
    {
        DISMISS = 1
    }
}