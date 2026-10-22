using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Operations.DTOs.PassengerManifest_SCVv7
{
    [GraphQLDescription("Passenger Manifest Response")]
    public class FlightDetails
    {
        [GraphQLDescription("How many stops, including final destination")]
        public string NumberOfStops { get; set; }
        [GraphQLDescription("Distance in ??")]
        public int? FlightDistance { get; set; }
        [GraphQLDescription("Does the flight arrive the following day")]
        public string NextDayIndicator { get; set; }
        [GraphQLDescription("Duraration of flight, in minutes for entire journey [includes any ground time at intermediate stops]")]
        public int FlightDuration { get; set; }
        [GraphQLDescription("Operating carrier of flight")]
        public string OperatingCarrier { get; set; }
        [GraphQLDescription("Operator's flight number")]
        public string OperatingFlightNumber { get; set; }
        [GraphQLDescription("Initial departing airport")]
        public string OriginAirportCode { get; set; }
        [GraphQLDescription("Final arriving airport")]
        public string DestinationAirportCode { get; set; }
        [GraphQLDescription("First leg local departure time")]
        public DateTime? LocalScheduledDepartureTime { get; set; }
        [GraphQLDescription("First leg utc departure time")]
        [GraphQLName("UTCScheduledDepartureTime")]
        public DateTime? UTCScheduledDepartureTime { get; set; }
        [GraphQLDescription("Final leg local arrival time")]
        public DateTime? LocalScheduledArrivalTime { get; set; }
        [GraphQLDescription("Final leg utc arriving time")]
        [GraphQLName("UTCScheduledArrivalTime")]
        public DateTime? UTCScheduledArrivalTime { get; set; }
        [GraphQLDescription("List of all the legs on the flight")]
        [UseSorting]
        public LegDetail[] LegDetails { get; set; }
        [GraphQLDescription("List of all customers on the flight")]
        [UseSorting]
        [UseFiltering]
        public CustomerDetail[] CustomerDetails { get; set; }
        public string DocumentVersion { get; set; }
        public DateTime? LoadDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
