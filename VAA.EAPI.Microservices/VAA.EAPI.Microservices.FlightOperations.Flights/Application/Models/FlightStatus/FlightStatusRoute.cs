using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRoute
    {
        public static explicit operator FlightStatusRoute?(DeltaRoute? source) => source is null
            ? null
            : new FlightStatusRoute
            {
                PointToPointStops = source.PointToPointStops,
                TotalRouteTime = source.TotalRouteTime,
                TotalRouteTimeFullText = source.TotalRouteTimeFullText,
                DiversionExists = source.DiversionExists,
                Segments = source.Segments?.Select(segment => (FlightStatusRouteSegment?)segment).ToArray()
            };

        public string? PointToPointStops { get; set; }
        public string? TotalRouteTime { get; set; }
        public string? TotalRouteTimeFullText { get; set; }
        public bool? DiversionExists { get; set; }
        public IEnumerable<FlightStatusRouteSegment?>? Segments { get; set; }
    }
}
