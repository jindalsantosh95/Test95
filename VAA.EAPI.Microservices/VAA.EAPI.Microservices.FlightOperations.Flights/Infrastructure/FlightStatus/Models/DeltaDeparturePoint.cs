namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaDeparturePoint(DeltaLeg? leg)
    {
        public DeltaLeg? Leg { get; } = leg;
    }
}
