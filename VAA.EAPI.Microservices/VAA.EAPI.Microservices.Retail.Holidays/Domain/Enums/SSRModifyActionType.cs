using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SSRModifyActionType
    {
        ADD = 1,
        EDIT,
        DELETE
    }
}