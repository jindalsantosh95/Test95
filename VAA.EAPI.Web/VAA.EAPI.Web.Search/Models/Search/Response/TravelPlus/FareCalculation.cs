using System.Collections.Generic;
/**
* The flight search fare calculation
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class FareCalculation
    {

        public string passengerType { get; set; }

        public Price fare { get; set; }

        public IEnumerable<Tax> taxes { get; set; }

    }
}
