using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class FlightDistance
    {
        public string? Unit { get; set; }
        public int? Distance { get; set; }

        public static explicit operator FlightDistance(JObject source)
        {
            return new FlightDistance()
            {
                Unit = source["Unit"]?.ToString(),
                Distance = source["Distance"] != null ? (int)source["Distance"] : null
            };
        }
    }
}
