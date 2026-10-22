/**
* The flight search slice fare segment
*
* @author Gareth Williams
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class SliceFareSegment
    {

        public string cabinName { get; set; }

        public string bookingClass { get; set; }

        public bool isDominantLeg { get; set; }
        public bool? isSaverFare { get; set; }

    }
}
