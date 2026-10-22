using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Operations.DTOs.PassengerManifest_SCVv7
{
    public class LegDetail
    {
        [GraphQLDescription("Operating carrier of this leg")]
        public string OperatingCarrierCode { get; set; }
        [GraphQLDescription("Operator's flight number of this leg")]
        public string OperatingFlightNumber { get; set; }
        [GraphQLDescription("Departure airport of this leg")]
        public string OriginAirportCode { get; set; }
        [GraphQLDescription("Arrival airport of this leg")]
        public string DestinationAirportCode { get; set; }
        [GraphQLDescription("Local departure time")]
        public DateTime? LocalScheduledDepartureTime { get; set; }
        [GraphQLDescription("UTC departure time of this leg")]
        [GraphQLName("UTCScheduledDepartureTime")]
        public DateTime? UTCScheduledDepartureTime { get; set; }
        [GraphQLDescription("Local arrival time of this leg")]
        public DateTime? LocalScheduledArrivalTime { get; set; }
        [GraphQLDescription("UTC arriving time of this leg")]
        [GraphQLName("UTCScheduledArrivalTime")]
        public DateTime? UTCScheduledArrivalTime { get; set; }
        [GraphQLDescription("Leg number")]
        public string LegSequenceNumber { get; set; }
    }
}
