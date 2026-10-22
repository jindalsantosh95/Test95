using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AffiliationCodeType
    {
        IP = 1
    }
}
