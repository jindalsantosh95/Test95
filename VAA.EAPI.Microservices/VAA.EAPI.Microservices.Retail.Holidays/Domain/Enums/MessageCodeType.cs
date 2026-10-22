using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageCodeType
    {
        NOTIFICATION = 0,
        IROP,
        SCHEDULE_CHANGE,
        FLYREADY,
        ADVISORY_MESSAGE
    }
}
