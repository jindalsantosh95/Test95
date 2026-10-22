/**
* The flight search slice fare segment
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class SliceFareSegment
    {

        public string cabinName { get; set; }

        public string bookingClass { get; set; }

        public bool isDominantLeg { get; set; }
        public string flightSegmentRefID { get; set; }
        public bool IsSaverFare { get; set; }
    }
}
