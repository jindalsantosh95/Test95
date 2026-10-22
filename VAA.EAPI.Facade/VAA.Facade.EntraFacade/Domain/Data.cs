using Newtonsoft.Json;

namespace VAA.Facade.EntraFacade.Domain
{
    public class Data
    {
        [JsonProperty("@odata.type")]
        public string DataType { get; set; }
        [JsonProperty("actions")]
        public IEnumerable<ResponseAction> Actions { get; set; }

        public Data(string dataType)
        {
            DataType = dataType;
        }
    }
}
