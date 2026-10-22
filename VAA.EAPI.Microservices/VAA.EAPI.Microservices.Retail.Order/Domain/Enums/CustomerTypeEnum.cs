using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomerTypeEnum
    {
        Basic = 0,
        Loyalty = 1
    }
}
