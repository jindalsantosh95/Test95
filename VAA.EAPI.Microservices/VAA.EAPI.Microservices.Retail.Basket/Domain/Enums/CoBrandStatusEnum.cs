using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CoBrandStatusEnum
    {
        NotApplied,

        Initiated,

        Approved,

        Rejected,

        Pending,

        ApprovedInsufficientCredit,

        Error
    }
}

