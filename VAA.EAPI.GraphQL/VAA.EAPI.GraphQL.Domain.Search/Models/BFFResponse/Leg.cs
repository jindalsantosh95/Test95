using NodaTime;
/**
* The flight search leg
*
* @author Gareth Williams
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    [GraphQLName("LegBFF")]
    public class Leg
    {

        public Airport origin { get; set; }

        public Airport destination { get; set; }

        public string duration { get; set; }

        public LocalDateTime? departure { get; set; }

        public LocalDateTime? arrival { get; set; }

        public string? stopOver { get; set; }

        public bool isDominantLeg { get; set; }
        
        public SegmentAirline? operatingAirline { get; set; }
        
        public SegmentAirline? marketingAirline { get; set; }

    }
}
