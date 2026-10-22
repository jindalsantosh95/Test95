using Newtonsoft.Json;
using System.Collections.Generic;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Airport;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Location
{
    public class PricingLocation
    {
        [JsonProperty(PropertyName = "AIRPORT_LIST")]
        public IEnumerable<AirportList> AirportListDetails { get; set; }
        [JsonProperty(PropertyName = "LOCATION_MAPPING")]
        public IEnumerable<LocationMapping> LocationMappingIEnumerableDetails { get; set; }
    }
}
