using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{

    public class CompartmentAvailability
    {
        public string Code { get; set; }
        public int Capacity { get; set; }
        public int? ActualAvailableInventory { get; set; }
        public int? AuthorisedAvailableInventory { get; set; }
        public int? InventorySold { get; set; }

        public static explicit operator CompartmentAvailability(JObject source)
        {
            return new CompartmentAvailability()
            {
                Code = source["Code"]?.ToString(),
                Capacity = source["Capacity"] != null ? (int)source["Capacity"] : 0,
                ActualAvailableInventory = source["ActualAvailableInventory"] != null ? (int)source["ActualAvailableInventory"] : null,
                AuthorisedAvailableInventory = source["AuthorisedAvailableInventory"] != null ? (int)source["AuthorisedAvailableInventory"] : null,
                InventorySold = source["InventorySold"] != null ? (int)source["InventorySold"] : null
            };
        }
    }
}
