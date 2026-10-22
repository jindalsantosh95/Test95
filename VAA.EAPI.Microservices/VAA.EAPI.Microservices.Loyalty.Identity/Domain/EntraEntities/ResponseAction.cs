using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities
{
    public class ResponseAction
    {
        [JsonPropertyName("@odata.type")]
        public string DataType { get; set; }
        [JsonPropertyName("claims")]
        public ClaimsResponse Claims { get; set; }

        public ResponseAction(string dataType, ClaimsResponse claimsResponse)
        {
            DataType = dataType;
            Claims = claimsResponse;
        }
    }
}
