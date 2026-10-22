using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS
{
    public class BaggagePolicy
    {
        public BagProperties Depricated { get; set; }
        public FreeBag FreeBags { get; set; }
        public IEnumerable<PaidBag> PaidBags { get; set; }
        public string Application { get; set; }
        public string Headline { get; set; }
        public string Description { get; set; }
        public string Assessment { get; set; }
        public string GroupCode { get; set; }
        public string SubGroupCode { get; set; }
        public string LargeImageLink { get; set; }
        public string SmallImageLink { get; set; }
    }
}
