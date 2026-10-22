namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight
{

    public class CompartmentAvailability
    {
        public string Code { get; set; }
        public int Capacity { get; set; }
        public int? ActualAvailableInventory { get; set; }
        public int? AuthorisedAvailableInventory { get; set; }
        public int? InventorySold { get; set; }
    }
}
