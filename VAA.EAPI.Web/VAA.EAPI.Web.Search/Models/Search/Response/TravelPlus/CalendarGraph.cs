using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class CalendarGraph
    {
        public IEnumerable<CalendarFromPrice> FromPrices { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }


    }

    public class CalendarFromPrice
    {
        public DateOnly FromDate { get; set; }
        public CalendarPrice Price { get; set; }
    }
}
