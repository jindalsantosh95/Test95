using Newtonsoft.Json;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class AirTicketing
    {
        [JsonProperty(PropertyName = "FOP_Qualifiers")]
        public FOPQualifiers FOPQualifiers { get; set; }
        public PricingQualifiers PricingQualifiers { get; set; }
    }
}
