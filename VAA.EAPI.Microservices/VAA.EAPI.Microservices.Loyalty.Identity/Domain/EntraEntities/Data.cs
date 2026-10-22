using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities
{
    public class Data
    {
        [JsonPropertyName("@odata.type")]
        public string DataType { get; set; }        
        [JsonPropertyName("actions")]
        public IEnumerable<ResponseAction> Actions { get; set; }

        public Data(string dataType)
        {
            DataType = dataType;
        }
    }
}
