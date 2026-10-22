using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Baggage
{
    public class BagDescriptor
    {
        public string BagDescriptorName { get; set; }
        public int BagCnt { get; set; }
        public List<string> BagDescriptorDesc { get; set; }
        public string BagDescriptorSubCode { get; set; }
    }
}
