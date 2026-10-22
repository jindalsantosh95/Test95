namespace VAA.EAPI.GraphQL.Domain.Search.Models.CarHire
{
    public class Car_CJS
    {
        public string? Id { get; set; }
        public string? CarClass { get; set; }
        public string? CarType { get; set; }
        public int Doors { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Seats { get; set; }
        public string? SupplierName { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public IEnumerable<string> Features { get; set; } = Enumerable.Empty<string>();
        public bool Abs { get; set; }
        public bool AirBags { get; set; }
        public bool AirConditioned { get; set; }
        public bool AutoTransmission { get; set; }
        public CarHireContent? content { get; set; }
        public float TotalPrice { get; set; }
        public float PricePerDay { get; set; }
        public IEnumerable<string> InsuranceIncludes { get; set; } = Enumerable.Empty<string>();
        public string? DepartureDate { get; set; }
    }

}
