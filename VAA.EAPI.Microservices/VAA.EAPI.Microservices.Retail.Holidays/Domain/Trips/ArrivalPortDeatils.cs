using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class ArrivalPortDeatils
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator ArrivalPortDeatils(JToken source)
        {
            if (source == null) return null;

            return new()
            {
                Code = source["code"]?.Value<string>(),
                Name = source["name"]?.Value<string>()
            };
        }
    }
}