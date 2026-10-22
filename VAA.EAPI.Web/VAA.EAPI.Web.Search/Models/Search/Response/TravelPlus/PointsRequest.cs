using System.Collections.Generic;
/**
* The flight search request
*
* @author James Twisleton
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class PointsRequest : Request
    {

        public IEnumerable<string> cabinClasses { get; set; }

    }
}
