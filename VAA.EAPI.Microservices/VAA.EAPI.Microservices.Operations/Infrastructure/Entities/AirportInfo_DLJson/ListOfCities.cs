using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Operations.Infrastructure.Entities.AirportInfo_DLJson
{
    public class ListOfCities
    {
        [JsonProperty("ListOfCities")]
        public AirportInfo[] Airports { get; set; }
    }
}
