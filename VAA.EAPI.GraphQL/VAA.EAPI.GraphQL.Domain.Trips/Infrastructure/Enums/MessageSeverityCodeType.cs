using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums
{
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum MessageSeverityCodeType
    {
        ACTION_REQUIRED = 0,
        NEEDS_ATTENTION,
        INFORMATIONAL
    }
}
