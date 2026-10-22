using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight
{
    [GraphQLName("LegsBook")]
    public class Legs
    {
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public string LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string OperatingCarrier { get; set; }
        public string MarketingCarrier { get; set; }
        public bool? IsDominantLeg { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public string DestinationAirportCode { get; set; }
        public string LegDuration { get; set; }
        public string LegId { get; set; }
        public FlightDistance Distance { get; set; }

    }
}
