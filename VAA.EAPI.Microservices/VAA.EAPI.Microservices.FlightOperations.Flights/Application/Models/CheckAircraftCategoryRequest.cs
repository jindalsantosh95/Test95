namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class CheckAircraftCategoryRequest
    {
        public IEnumerable<string> TailNumbers { get; set; } = [];

        public int CategoryIndex { get; set; }
    }
}
