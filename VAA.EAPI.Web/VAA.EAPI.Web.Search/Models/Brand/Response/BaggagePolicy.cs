using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class BaggagePolicy : SegmentPolicy
    {
        public BagProperties depricated { get; set; }
        public FreeBag FreeBags { get; set; }
        public IEnumerable<PaidBag> PaidBags { get; set; }
    }
}