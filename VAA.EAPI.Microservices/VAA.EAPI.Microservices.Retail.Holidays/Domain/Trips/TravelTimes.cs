using Newtonsoft.Json.Linq;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class TravelTimes
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public static explicit operator TravelTimes(JToken source)
        {
            if (source == null)
                return null;

            return new()
            {
                EndTime = source["endTime"]?.Value<DateTime>(),
                StartTime = source["startTime"]?.Value<DateTime>()
            };
        }
    }
}
