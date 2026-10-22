using Newtonsoft.Json.Linq;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class TravelDatesInfo
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public static explicit operator TravelDatesInfo(JToken source)
        {
            if (source == null)
                return null;

            return new()
            {
                EndDate = source["endDate"]?.Value<DateTime>(),
                StartDate = source["startDate"]?.Value<DateTime>()
            };
        }
    }
}
