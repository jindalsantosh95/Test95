using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities
{
    public class RequestData
    {
        [JsonPropertyName("@odata.type")]
        public string DataType { get; set; }
        public OtpContext OtpContext { get; set; }
        public AuthenticationContext AuthenticationContext { get; set; }
    }
}
