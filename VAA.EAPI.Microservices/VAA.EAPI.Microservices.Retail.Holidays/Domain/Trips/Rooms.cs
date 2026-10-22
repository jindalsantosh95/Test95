using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Rooms
    {
        public string RoomName { get; set; }
        public string RoomType { get; set; }
        public string BoardBasis { get; set; }
        public string RoomNumber { get; set; }
        public int RoomCount { get; set; }
        public int PaxCount { get; set; }
        public TravellerInfo TravellerInfo { get; set; }
        public RateInfo Rate { get; set; }
        public string MealPlanName { get; set; }

        public static explicit operator Rooms(JToken source)
        {
            if (source == null) return null;
            return new Rooms()
            {
                RoomType = source["typeName"]?.Value<string>(),
                BoardBasis = source["mealPlanName"]?.Value<string>(),
                RoomNumber = source["roomNo"]?.Value<string>(),
                RoomCount = source["roomCount"]?.Value<int>() ?? default,
                PaxCount = source["travellerInfo"]?["adult"]?.Value<int>() + source["travellerInfo"]?["child"]?.Value<int>() + source["travellerInfo"]?["infant"]?.Value<int>() ?? default,
                TravellerInfo = (TravellerInfo)source["travellerInfo"],
                Rate = (RateInfo)source["rate"],
                MealPlanName = source["mealPlanName"]?.Value<string>()
            };
        }
    }
}