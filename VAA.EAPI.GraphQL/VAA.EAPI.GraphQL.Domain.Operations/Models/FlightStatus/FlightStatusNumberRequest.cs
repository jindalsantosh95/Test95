#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus
{
    public sealed class FlightStatusNumberRequest
    {
        public string FlightNumber { get; init; } = null!;
        public DateOnly DepartureDate { get; init; }
        public string DepartureAirportCode { get; init; } = null!;
        public string? SelectedLegId { get; init; }
    }
}
