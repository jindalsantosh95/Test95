using Newtonsoft.Json.Linq;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Aircraft
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SubfleetCode { get; set; }
        public string PlaneName { get; set; }
        public string TailNumber { get; set; }

        public static explicit operator Aircraft(JToken source)
        {
            if (source == null) return null;

            return new()
            {
                SubfleetCode = source["equipment"]?.Value<string>(),
            };
        }
    }
}
