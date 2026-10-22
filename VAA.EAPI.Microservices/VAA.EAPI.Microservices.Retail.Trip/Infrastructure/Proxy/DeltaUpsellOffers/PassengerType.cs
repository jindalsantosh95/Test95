using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    [JsonConverter(typeof(StringEnumConverter))]
    public class PassengerType
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public PassengerAgeType Category { get; set; }
    }
}
