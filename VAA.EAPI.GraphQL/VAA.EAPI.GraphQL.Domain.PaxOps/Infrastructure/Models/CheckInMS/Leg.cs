using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS;

public class Leg
{
    public string LegId { get; set; }
    public int? Ordinal { get; set; }
    public LocalDateTime ScheduledDepartureLocalDateTime { get; set; }
    public LocalDateTime ScheduledDepartureUtcDateTime { get; set; }
    public LocalDateTime ScheduledArrivalLocalDateTime { get; set; }
    public LocalDateTime ScheduledArrivalUtcDateTime { get; set; }
    public AirportAndTerminal Origin { get; set; }
    public AirportAndTerminal Destination { get; set; }
    public int? DistanceMiles { get; set; }
    public string? FlightDuration { get; set; }
    public string AircraftTypeName { get; set; }
    public string Status { get; set; }
}