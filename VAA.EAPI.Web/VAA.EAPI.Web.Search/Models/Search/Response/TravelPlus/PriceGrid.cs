using System;
using System.Collections.Generic;
/**
* The flight search price grid
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class PriceGrid
    {

        public IEnumerable<DateOnly> returning { get; set; }

        public IEnumerable<PriceGridDeparture> departures { get; set; }

    }
}
