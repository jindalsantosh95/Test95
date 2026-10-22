namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusLegDetail
    {
        public FlightStatusAircraft? Aircraft { get; set; }
        public FlightStatusInfo? Status { get; set; }
        public FlightStatusPerformance? Performance { get; set; }
        public FlightStatusPosition? Position { get; set; }
        public IEnumerable<FlightStatusAmenity?>? Amenities { get; set; }
        public IEnumerable<FlightStatusMeal?>? Meals { get; set; }
    }
}
