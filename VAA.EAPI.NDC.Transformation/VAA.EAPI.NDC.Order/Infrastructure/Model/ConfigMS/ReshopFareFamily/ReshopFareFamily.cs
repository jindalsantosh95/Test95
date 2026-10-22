using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.ReshopFareFamily
{
    public class ReshopFareFamily
    {
        [JsonProperty(PropertyName = "CABINCODE")]
        public string CabinCode { get; set; }
        [JsonProperty(PropertyName = "EXISTING_FAREFAMILY")]
        public string ExistingFareFamily { get; set; }
        [JsonProperty(PropertyName = "ITA_FAREFAMILIES")]
        public IEnumerable<string> ItaFareFamilies { get; set; }
    }
}
