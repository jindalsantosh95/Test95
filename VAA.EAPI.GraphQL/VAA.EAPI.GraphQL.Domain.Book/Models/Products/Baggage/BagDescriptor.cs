namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Baggage
{
    public class BagDescriptor
    {
        public string BagDescriptorName { get; set; }
        public int BagCnt { get; set; }
        public IEnumerable<string> BagDescriptorDesc { get; set; }
        public string BagDescriptorSubCode { get; set; }
    }
}
