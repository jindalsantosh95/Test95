using NodaTime;

/**
* The flight search segment
*
* @author Gareth Williams
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    [GraphQLName("SegmentBFF")]
    public class Segment
    {

        public IEnumerable<FlightSearchMetal> metal { get; set; }

        public SegmentAirline airline { get; set; }

        public string flightNumber { get; set; }

        public string operatingFlightNumber { get; set; }

        public SegmentAirline operatingAirline { get; set; }

        public Airport origin { get; set; }

        public Airport destination { get; set; }

        public string duration { get; set; }

        public LocalDateTime? departure { get; set; }

        public LocalDateTime? arrival { get; set; }

        public int stopCount { get; set; }

        public string? connection { get; set; }

        public IEnumerable<Leg> legs { get; set; }

        public string? bookingClass { get; set; }

        public string? fareBasisCode { get; set; }

        public string? dominantFareProduct { get; set; }

        public bool? pendingGovtApproval { get; set; }


    }
}
