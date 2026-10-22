using Newtonsoft.Json;

namespace VAA.Facade.EntraFacade.Domain
{
    public class ResponseObject
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        public ResponseObject(string dataType)
        {
            Data = new Data(dataType);
        }
    }
}
