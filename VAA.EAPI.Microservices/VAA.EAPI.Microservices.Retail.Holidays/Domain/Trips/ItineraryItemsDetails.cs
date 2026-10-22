using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class ItineraryItemsDetails
    {
        public int ItemNo { get; set; }
        public string Description { get; set; }
        public int FromDay { get; set; }
        public int ToDay { get; set; }

        public static explicit operator ItineraryItemsDetails(JToken source)
        {
            if (source == null)
                return null;

            return new()
            {
                Description = source["description"]?.Value<string>(),
                FromDay = source["fromDay"]?.Value<int>() ?? default,
                ToDay = source["toDay"]?.Value<int>() ?? default,
                ItemNo = source["itemNo"]?.Value<int>() ?? default,
            };
        }
    }
}