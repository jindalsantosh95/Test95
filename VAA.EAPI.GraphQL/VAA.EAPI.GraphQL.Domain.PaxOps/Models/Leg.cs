using HotChocolate;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models;

[GraphQLName("PaxOps:Leg")]
public class Leg
{
    public string LegId { get; set; }
    public LocalDateTime ScheduledDepartureLocalDateTime { get; set; }
    public LocalDateTime ScheduledDepartureUtcDateTime { get; set; }
    public LocalDateTime ScheduledArrivalLocalDateTime { get; set; }
    public LocalDateTime ScheduledArrivalUtcDateTime { get; set; }

    [GraphQLDescription("Leg origin airport and terminal.")]
    public AirportAndTerminal Origin { get; set; }

    [GraphQLDescription("Leg destination airport and terminal.")]
    public AirportAndTerminal Destination { get; set; }

    [GraphQLDescription("Distance in miles when available.")]
    public int? DistanceMiles { get; set; }

    [GraphQLDescription("Flight duration display string when available. Typically <hours>H <minutes>M, for example 7H 40M, but clients should not rely on this exact format.")]
    public string? FlightDuration { get; set; }

    public string AircraftTypeName { get; set; }

    public string Status { get; set; }

    public static explicit operator Leg(Infrastructure.Models.CheckInMS.Leg source) => new()
    {
        LegId = source.LegId,
        ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
        ScheduledDepartureUtcDateTime = source.ScheduledDepartureUtcDateTime,
        ScheduledArrivalLocalDateTime = source.ScheduledArrivalLocalDateTime,
        ScheduledArrivalUtcDateTime = source.ScheduledArrivalUtcDateTime,
        Origin = source.Origin != null ? (AirportAndTerminal)source.Origin : null,
        Destination = source.Destination != null ? (AirportAndTerminal)source.Destination : null,
        DistanceMiles = source.DistanceMiles,
        FlightDuration = source.FlightDuration,
        AircraftTypeName = source.AircraftTypeName,
        Status = source.Status,
    };
}
