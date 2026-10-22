using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.UpsellOffer
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OfferCodeType
    {
        CASH = 1,
        MILES = 2
    }
}