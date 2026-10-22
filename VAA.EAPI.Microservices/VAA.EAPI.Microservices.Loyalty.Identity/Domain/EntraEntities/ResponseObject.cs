using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities
{
    public class ResponseObject
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        public ResponseObject(string dataType)
        {
            Data = new Data(dataType);
        }
    }
}
