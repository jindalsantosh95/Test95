namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaArrivalPoint(DeltaLeg? leg)
    {
        public DeltaLeg? Leg { get; } = leg;
    }
}
