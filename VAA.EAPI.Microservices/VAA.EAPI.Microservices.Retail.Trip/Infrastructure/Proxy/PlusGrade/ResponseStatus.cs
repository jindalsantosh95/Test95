using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.PlusGrade
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseStatus
    {
        ELIGIBLE = 1,
        NOT_ELIGIBLE = 2,
        PNR_NOT_FOUND = 3
    }
}