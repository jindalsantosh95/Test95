using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HolidayBookingStatus
    {
        CONFIRMED = 1000,
        CANCELLED = 2000,
        QUOTE = 6000
    }
}
