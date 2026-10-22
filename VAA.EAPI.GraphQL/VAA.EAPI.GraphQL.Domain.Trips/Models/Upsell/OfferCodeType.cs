using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OfferCodeType
    {
        CASH = 1,
        MILES = 2
    }
}