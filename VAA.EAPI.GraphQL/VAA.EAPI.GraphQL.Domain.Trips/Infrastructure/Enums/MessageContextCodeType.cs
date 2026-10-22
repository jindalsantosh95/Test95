using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageContextCodeType
    {
        PNR,
        TRIP,
        SEGMENT
    }
}
