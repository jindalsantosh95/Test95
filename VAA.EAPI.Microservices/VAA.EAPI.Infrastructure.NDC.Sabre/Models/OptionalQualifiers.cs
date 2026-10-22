using Newtonsoft.Json;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class OptionalQualifiers
    {
        [JsonProperty(PropertyName = "FOP_Qualifiers")]
        public FOPQualifiers FOPQualifiers { get; set; }
        public PricingQualifiers PricingQualifiers { get; set; }
    }
}
