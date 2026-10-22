using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class FilterCollection
    {
        public IEnumerable<Filter> Filters { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
        public IEnumerable<Link> Links { get; set; } = Enumerable.Empty<Link>();
    }
}
