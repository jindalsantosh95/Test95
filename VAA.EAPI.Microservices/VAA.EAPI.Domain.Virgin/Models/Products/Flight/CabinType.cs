using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class CabinType
    {
        public string CabinCode { get; set; }
        public string CabinName { get; set; }

        public static explicit operator CabinType(JObject source)
        {
            return new CabinType()
            {
                CabinCode = source["CabinCode"]?.ToString(),
                CabinName = source["CabinName"]?.ToString()
            };
        }
    }
}
