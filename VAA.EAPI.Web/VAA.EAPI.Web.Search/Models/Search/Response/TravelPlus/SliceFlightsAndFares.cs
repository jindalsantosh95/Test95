using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class SliceFlightsAndFares
    {
        public SliceFlight Flight { get; set; }
        public IEnumerable<SliceFare> Fares { get; set; }
    }
}
