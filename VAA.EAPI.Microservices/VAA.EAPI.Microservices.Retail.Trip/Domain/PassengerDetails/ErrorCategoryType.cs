using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ErrorCategoryType
    {
        PREVALIDATE = 1,
        VALIDATE = 2,
        ERROR = 3,
        NOTIFY = 4
    }
}