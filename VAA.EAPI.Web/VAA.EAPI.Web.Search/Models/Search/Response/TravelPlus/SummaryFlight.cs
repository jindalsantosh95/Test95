using System.Collections.Generic;
/**
* The flight search flight
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class SummaryFlight
    {

        public string descriptor { get; set; }

        public string fareDescriptor { get; set; }

        public IEnumerable<Segment> segments { get; set; }

        public string duration { get; set; }

        public string fareFamilyType { get; set; }

        public string cabinName { get; set; }

    }
}
