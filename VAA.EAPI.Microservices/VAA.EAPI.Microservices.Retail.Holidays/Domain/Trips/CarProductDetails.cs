using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class CarProductDetails
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public RateInfo Rate { get; set; }

        public static explicit operator CarProductDetails?(JToken? source)
        {
            if (source is null)
                return null;

            return new()
            {
                Id = source["id"]?.Value<int>(),
                Code = source["code"]?.Value<string>(),
                Name = source["name"]?.Value<string>(),
                Rate = (RateInfo)source["rate"]
            };
        }
    }
}