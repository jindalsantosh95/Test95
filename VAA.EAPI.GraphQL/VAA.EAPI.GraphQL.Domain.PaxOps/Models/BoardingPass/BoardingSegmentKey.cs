using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass
{
    public sealed record BoardingSegmentKey(string PassengerCustomerId, string Origin, string Destination, string OperatingFlightNumber, LocalDateTime DepartureLocal);
}
