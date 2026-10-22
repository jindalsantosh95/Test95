namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaSegment
    {
        public string? EnrouteStops { get; set; }
        public bool? ChangeOfGauge { get; set; }
        public bool? CodeShare { get; set; }
        public bool? ConnectionCarrier { get; set; }
        public bool? ConnectionCarrierOrCodeShareOperator { get; set; }
        public string? ConnectionCarrierOrCodeShareOperatorName { get; set; }
        public IEnumerable<DeltaLeg>? Legs { get; set; }
        public IEnumerable<DeltaLeg>? LegsWithChangeOfGauge { get; set; }
    }
}
