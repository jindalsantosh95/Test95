using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums
{
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum MessageSeverityCodeType
    {
        ACTION_REQUIRED = 0,
        NEEDS_ATTENTION,
        INFORMATIONAL
    }
}
