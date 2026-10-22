using Newtonsoft.Json;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class BasicFOP
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "CC_Info")]
        public CCInfo CCInfo { get; set; }
    }
}
