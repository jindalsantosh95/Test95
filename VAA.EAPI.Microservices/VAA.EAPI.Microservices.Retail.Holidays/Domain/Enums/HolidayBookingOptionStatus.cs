using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HolidayBookingOptionStatus
    {
        Quote = 200,
        Definite = 400,
        Firm = 500,
        Fully_Paid = 600,
        Settled = 700
    }
}
