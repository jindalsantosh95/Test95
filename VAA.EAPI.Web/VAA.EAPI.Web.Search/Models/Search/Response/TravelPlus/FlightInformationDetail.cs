using System.Collections.Generic;
/**
* The flight search flight information slice
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class FlightInformationDetail
    {

        public IEnumerable<FlightInformationDetailCategory> categories { get; set; }

    }
}
