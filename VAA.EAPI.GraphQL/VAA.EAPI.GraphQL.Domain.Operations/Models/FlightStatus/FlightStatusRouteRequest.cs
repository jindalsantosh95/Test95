#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus
{
    public sealed class FlightStatusRouteRequest
    {
        public string DepartureAirportCode { get; init; } = null!;
        public string ArrivalAirportCode { get; init; } = null!;
        public DateOnly ScheduledDepartureDate { get; init; }
        public IEnumerable<int>? TimeRange { get; init; }
        public IEnumerable<int>? Stops { get; init; }
        public bool? First { get; init; }
    }
}
