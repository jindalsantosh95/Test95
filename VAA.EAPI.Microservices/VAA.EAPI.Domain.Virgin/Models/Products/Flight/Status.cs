using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class Status
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string DeliveryStatusCode { get; set; }

        public static explicit operator Status(JObject source)
        {
            return new Status()
            {
                Code = source["Code"]?.ToString(),
                Description = source["Description"]?.ToString(),
                DeliveryStatusCode = source["DeliveryStatusCode"]?.ToString()
            };
        }
    }
}
