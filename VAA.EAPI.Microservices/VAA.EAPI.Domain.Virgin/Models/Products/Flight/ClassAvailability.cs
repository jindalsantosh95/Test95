using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class ClassAvailability
    {
        public string Compartment { get; set; }
        public string Class { get; set; }
        public int? AuthorisedAvailability { get; set; }
        public bool? ProhibitSalesIndicator { get; set; }
        public int Hierachy { get; set; }
        public bool WaitlistOpen { get; set; }

        public static explicit operator ClassAvailability(JObject source)
        {
            return new ClassAvailability()
            {
                Compartment = source["Compartment"]?.ToString(),
                Class = source["Class"]?.ToString(),
                AuthorisedAvailability = source["AuthorisedAvailability"] != null ? (int)source["AuthorisedAvailability"] : null,
                ProhibitSalesIndicator = source["ProhibitSalesIndicator"] != null ? (bool)source["ProhibitSalesIndicator"] : null,
                Hierachy = source["Hierachy"] != null ? (int)source["Hierachy"] : 0,
                WaitlistOpen = source["WaitlistOpen"] != null ? (bool)source["WaitlistOpen"] : false,
            };
        }
    }
}
