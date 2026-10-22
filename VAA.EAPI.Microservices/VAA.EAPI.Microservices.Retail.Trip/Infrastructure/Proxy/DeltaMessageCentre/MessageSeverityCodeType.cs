using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum MessageSeverityCodeType
    {
        ACTION_REQUIRED = 0,
        NEEDS_ATTENTION,
        INFORMATIONAL
    }
}
