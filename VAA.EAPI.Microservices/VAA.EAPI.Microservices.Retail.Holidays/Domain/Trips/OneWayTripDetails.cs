using Newtonsoft.Json.Linq;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class OneWayTripDetails
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int Duration { get; set; }

        public static explicit operator OneWayTripDetails(JToken source)
        {
            if (source == null)
                return null;

            return new()
            {
                Start = source["start"]?.Value<DateTime>(),
                End = source["end"]?.Value<DateTime>(),
                Duration = source["duration"]?.Value<int>() ?? default
            };
        }
    }
}
