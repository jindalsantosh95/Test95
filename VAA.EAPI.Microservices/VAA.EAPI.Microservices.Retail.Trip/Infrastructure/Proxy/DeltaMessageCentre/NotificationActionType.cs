using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NotificationActionType
    {
        
        DISMISS = 0
    }
}
