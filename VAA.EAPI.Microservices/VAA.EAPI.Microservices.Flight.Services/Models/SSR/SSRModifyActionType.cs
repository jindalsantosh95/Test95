using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SSRModifyActionType
    {
        ADD = 1,
        EDIT,
        DELETE
    }
}
