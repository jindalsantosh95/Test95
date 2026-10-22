using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class StoreLocationDetails
    {
        public string? Name { get; set; }
        public string? Value { get; set; }

        public static explicit operator StoreLocationDetails(JToken source)
        {
            if (source == null) return default;

            return new()
            {
                Name = source["name"]?.Value<string>(),
                Value = source["value"]?.Value<string>(),
            };
        }
    }
}