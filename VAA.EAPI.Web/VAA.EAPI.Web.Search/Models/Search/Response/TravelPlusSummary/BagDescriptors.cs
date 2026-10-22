using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class BagDescriptors
    {
        public string BagDescriptorName { get; set; }
        public int BagCnt { get; set; }
        public IEnumerable<string> BagDescriptorDesc { get; set; }
        public string BagDescriptorSubCode { get; set; }
    }
}
