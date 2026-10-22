using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities
{
    public class AmperityResponse
    {

        public string Id { get; set; }

        [JsonProperty("index_id")]
        public string IndexId { get; set; }
        public AttributeResponse Attributes { get; set; }       
    }
}
