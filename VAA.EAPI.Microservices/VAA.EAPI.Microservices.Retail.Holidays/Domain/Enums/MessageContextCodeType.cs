using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageContextCodeType
    {
        PNR,
        TRIP,
        SEGMENT
    }
}
