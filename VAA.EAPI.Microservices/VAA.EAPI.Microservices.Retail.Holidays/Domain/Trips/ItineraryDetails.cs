using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class ItineraryDetails
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public IEnumerable<ItineraryItemsDetails> ItineraryItems { get; set; }

        public static explicit operator ItineraryDetails(JToken source)
        {
            if (source == null) return null;

            return new()
            {
                Code = source["code"]?.Value<string>(),
                Name = source["name"]?.Value<string>(),
                ItineraryItems = source["itineraryItems"]?.Select(it => (ItineraryItemsDetails)it)
            };
        }
    }
}