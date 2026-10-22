using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class FlightInformation
    {
        public IEnumerable<FlightInformationRules> Rules { get; set; }
        public IEnumerable<Link> links { get; set; } = Enumerable.Empty<Link>();
    }
}
