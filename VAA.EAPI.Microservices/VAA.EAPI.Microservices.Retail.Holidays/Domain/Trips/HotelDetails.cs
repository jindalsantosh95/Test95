using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class HotelDetails
    {
        public IEnumerable<Rooms> RoomDetails { get; set; }
        public string Postcode { get; set; }
        public string Name { get; set; }
        public string StreetName { get; set; }
        public string TownCity { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public bool VipBooking { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string HotelCode { get; set; }
        public string AccommodationType { get; set; }
        public string HotelAddress { get; set; }
        public Location Destination { get; set; }
        public int? NumberOfRooms { get; set; }

        public static explicit operator HotelDetails(JToken source)
        {
            if (source == null || source["summary"] == null) return null;

            var hotelSummary = source["summary"];

            return new HotelDetails()
            {
                Name = hotelSummary["name"]?.Value<string>(),
                VipBooking = hotelSummary["vipBooking"]?.Value<bool>() ?? false,
                RoomDetails = (source["roomSet"]?["rooms"] as JArray)?.OfType<JObject>()?.Select(r => (Rooms)r),
                StartDate = hotelSummary["startTime"]?.Value<DateTime>(),
                EndDate = hotelSummary["endTime"]?.Value<DateTime>(),
                HotelAddress = source["detail"]?["address"]?.Value<string>(),
                HotelCode = source["detail"]?["hotelCode"]?.Value<string>(),
                AccommodationType = source["detail"]?["accomType"]?.Value<string>(),
                Destination = (Location)source["detail"]?["destination"],
                NumberOfRooms = (source["roomSet"]?["rooms"] as JArray)?.Count
            };
        }
    }
}
