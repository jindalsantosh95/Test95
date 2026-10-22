using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Location
{
    public class PricingLocation
    {
        [JsonProperty(PropertyName = "AIRPORT_LIST")]
        public IEnumerable<AirportList> AirportListDetails { get; set; }

        [JsonProperty(PropertyName = "LOCATION_MAPPING")]
        public IEnumerable<LocationMapping> LocationMappingIEnumerableDetails { get; set; }
    }
}
