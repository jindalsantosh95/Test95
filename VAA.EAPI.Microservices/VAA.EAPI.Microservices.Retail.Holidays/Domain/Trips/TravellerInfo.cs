using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class TravellerInfo
    {
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public int InfantCount { get; set; }
        public IEnumerable<TravellerParty> Travellers { get; set; }

        public static explicit operator TravellerInfo(JToken source)
        {
            if (source == null)
                return null;

            return new()
            {
                AdultCount = source["adult"]?.Value<int>() ?? 0,
                ChildCount = source["child"]?.Value<int>() ?? 0,
                InfantCount = source["infant"]?.Value<int>() ?? 0,
                Travellers = source["travellers"]?.Select(t => (TravellerParty)t)
            };
        }
    }
}
