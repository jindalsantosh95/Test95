using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SSRModifyActionType
    {
        ADD = 1,
        EDIT,
        DELETE
    }
}