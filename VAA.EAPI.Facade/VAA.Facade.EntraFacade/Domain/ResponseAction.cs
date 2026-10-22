using Newtonsoft.Json;

namespace VAA.Facade.EntraFacade.Domain
{
    public class ResponseAction
    {
        [JsonProperty("@odata.type")]
        public string DataType { get; set; }
        [JsonProperty("claims")]
        public ClaimsResponse Claims { get; set; }

        public ResponseAction(string dataType, ClaimsResponse claimsResponse)
        {
            DataType = dataType;
            Claims = claimsResponse;
        }
    }
}
