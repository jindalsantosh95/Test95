using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SSRModifyActionType
    {
        ADD = 1,
        EDIT,
        DELETE
    }
}
