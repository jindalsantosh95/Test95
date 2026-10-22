namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
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

