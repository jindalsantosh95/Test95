using Newtonsoft.Json;

namespace VAA.Facade.EntraFacade.Domain
{
    public class RequestData
    {
        [JsonProperty("@odata.type")]
        public string DataType { get; set; }
        public OtpContext OtpContext { get; set; }
        public AuthenticationContext AuthenticationContext { get; set; }
    }
}
