using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteSegment
    {
        public static explicit operator FlightStatusRouteSegment?(DeltaSegment? source) => source is null
            ? null
            : new FlightStatusRouteSegment
            {
                EnrouteStops = source.EnrouteStops,
                ChangeOfGauge = source.ChangeOfGauge,
                CodeShare = source.CodeShare,
                ConnectionCarrier = source.ConnectionCarrier,
                ConnectionCarrierOrCodeShareOperator = source.ConnectionCarrierOrCodeShareOperator,
                ConnectionCarrierOrCodeShareOperatorName = source.ConnectionCarrierOrCodeShareOperatorName,
                Legs = source.Legs?.Select(leg => (FlightStatusRouteLeg?)leg).ToArray(),
                LegsWithChangeOfGauge = source.LegsWithChangeOfGauge?.Select(leg => (FlightStatusRouteLeg?)leg).ToArray()
            };

        public string? EnrouteStops { get; set; }
        public bool? ChangeOfGauge { get; set; }
        public bool? CodeShare { get; set; }
        public bool? ConnectionCarrier { get; set; }
        public bool? ConnectionCarrierOrCodeShareOperator { get; set; }
        public string? ConnectionCarrierOrCodeShareOperatorName { get; set; }
        public IEnumerable<FlightStatusRouteLeg?>? Legs { get; set; }
        public IEnumerable<FlightStatusRouteLeg?>? LegsWithChangeOfGauge { get; set; }
    }
}
