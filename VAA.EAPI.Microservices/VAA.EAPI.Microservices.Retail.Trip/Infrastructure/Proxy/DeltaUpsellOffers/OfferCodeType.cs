using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OfferCodeType
    {
        CASH = 1,
        MILES = 2
    }
}
