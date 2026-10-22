using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class CabinInformation
    {
        public IEnumerable<CabinDetail> CabinDetails { get; set; }
        public int Segments { get; set; }
        public IEnumerable<Link> Links { get; set; } = Enumerable.Empty<Link>();
    }
}
