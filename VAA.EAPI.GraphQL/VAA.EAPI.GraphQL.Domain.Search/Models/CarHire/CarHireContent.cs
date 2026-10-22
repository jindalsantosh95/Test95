namespace VAA.EAPI.GraphQL.Domain.Search.Models.CarHire
{
    public class CarHireContent
    {
        public string? UrlName { get; set; }
        public string? Doors { get; set; }
        public string? Model { get; set; }
        public string? Make { get; set; }
        public LuggageCJS? Luggage { get; set; }
        public bool AirConditioning { get; set; }
        public string? SatelliteNavigation { get; set; }
        public string? Transmission { get; set; }
        public string? CarClass { get; set; }
        public Capacity_CJS? Capacity { get; set; }
        public CarImage2? Image { get; set; }
        public string? SupplierImage { get; set; }
    }

}
