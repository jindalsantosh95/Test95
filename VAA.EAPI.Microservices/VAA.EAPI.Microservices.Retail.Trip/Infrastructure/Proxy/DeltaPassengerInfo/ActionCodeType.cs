using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionCodeType
    {
        ADD = 1,
        DELETE = 2
    }
}