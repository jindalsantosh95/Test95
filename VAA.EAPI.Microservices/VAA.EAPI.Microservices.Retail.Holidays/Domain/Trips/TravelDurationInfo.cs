using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class TravelDurationInfo
    {
        public string Duration { get; set; }
        public string DurationType { get; set; }

        public static explicit operator TravelDurationInfo(JToken source)
        {
            if (source == null)
                return null;

            return new()
            {
                Duration = source["duration"]?.Value<string>(),
                DurationType = source["durationType"]?.Value<string>(),
            };
        }
    }
}
