using System.Collections.Generic;
/**
* The flight search flight information
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class FlightInformationRules
    {

        public IEnumerable<FlightInformationSegment> segments { get; set; }

        public FlightInformationDetail detail { get; set; }

        public string fareBasisCode { get; set; }

    }
}
