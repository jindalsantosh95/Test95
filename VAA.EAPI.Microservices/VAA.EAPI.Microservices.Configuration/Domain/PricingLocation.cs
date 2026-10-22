using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class PricingLocation
    {
        [JsonProperty(PropertyName = "AIRPORT_LIST")]
        public IEnumerable<AirportList> AirportListDetails { get; set; }
        [JsonProperty(PropertyName = "LOCATION_MAPPING")]
        public IEnumerable<LocationMapping> LocationMappingListDetails { get; set; }
    }
}
