using Newtonsoft.Json;

namespace VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2
{
    public class CosmosDocument
    {
        [JsonProperty(PropertyName = "id")]  // using jsonproperty as DataMember is not working for id attribute
        public string Id { get; set; }
        public string PartitionKey { get; set; }


    }
}
