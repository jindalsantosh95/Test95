namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaAmenitiesAndMealsResponse
    {
        public IEnumerable<DeltaAmenity?>? Amenities { get; set; }
        public IEnumerable<DeltaMeal?>? Meals { get; set; }
    }
}
