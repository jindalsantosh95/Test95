using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NotificationCodeType
    {

        FLIGHT,

        SEAT
    }
}
