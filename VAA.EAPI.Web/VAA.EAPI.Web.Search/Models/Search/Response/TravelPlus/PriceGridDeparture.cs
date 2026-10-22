using System;
using System.Collections.Generic;
/**
* The flight search price grid departures
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class PriceGridDeparture
    {

        public DateOnly departing { get; set; }

        public IEnumerable<PriceGridPrice> prices { get; set; }
    }
}
