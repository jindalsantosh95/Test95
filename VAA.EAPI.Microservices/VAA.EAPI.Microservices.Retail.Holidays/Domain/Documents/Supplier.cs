using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class Supplier
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Supplier(JObject source)
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
