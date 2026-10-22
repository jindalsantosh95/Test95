using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class Content
    {
        public string CabinName { get; set; }
        public string UpgradeImage { get; set; }
        public IEnumerable<Feature> Features { get; set; }
    }
}