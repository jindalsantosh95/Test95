using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionCodeType
    {
        ADD = 1,
        DELETE = 2
    }
}