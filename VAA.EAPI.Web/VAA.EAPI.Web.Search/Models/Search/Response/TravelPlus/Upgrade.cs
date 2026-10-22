using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Upgrade
    {
        public string Descriptor { get; set; }
        public string FareDescriptor { get; set; }
        public string FareFamilyType { get; set; }
        public Price Price { get; set; }
        public UpgradeContent Content { get; set; }
        public IEnumerable<Link> Links { get; set; } = Enumerable.Empty<Link>();
    }
}
