namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaRouteSummary
    {
        public string? DefaultRouteResultsStopsIndicator { get; set; }
        public bool? MultiStop { get; set; }
        public bool? NonStop { get; set; }
        public bool? OneStop { get; set; }
        public bool? TimeRangeOne { get; set; }
        public bool? TimeRangeTwo { get; set; }
        public bool? TimeRangeThree { get; set; }
    }
}
