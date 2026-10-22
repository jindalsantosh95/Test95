using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class Reshop_FareFamily
    {
        [JsonProperty(PropertyName = "CABINCODE")]
        public string CabinCode { get; set; }
        [JsonProperty(PropertyName = "EXISTING_FAREFAMILY")]
        public string ExistingFareFamily { get; set; }
        [JsonProperty(PropertyName = "ITA_FAREFAMILIES")]
        public IEnumerable<string> ItaFareFamilies { get; set; }

    }
}
