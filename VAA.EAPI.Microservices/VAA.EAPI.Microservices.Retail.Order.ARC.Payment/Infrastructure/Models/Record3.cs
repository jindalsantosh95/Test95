using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Record3
    {
        [JsonProperty("TDOI")]
        public string TDOI { get; set; }

        [JsonProperty("RELATED_DOC_ONE")]
        public RelatedDocOne RelatedDocOne { get; set; }
    }
}
