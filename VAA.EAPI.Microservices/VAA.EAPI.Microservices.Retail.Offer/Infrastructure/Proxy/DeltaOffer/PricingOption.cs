using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class PricingOption
    {
        public string PricingOptionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PricingOptionTypeTextEnum
        {
            [EnumMember(Value = "CASH")]
            CASH = 1,
            [EnumMember(Value = "MILES")]
            MILES = 2
        }
        public PricingOptionTypeTextEnum PricingOptionTypeText { get; set; }
        public PricingOptionDetail PricingOptionDetail { get; set; }
    }
}
