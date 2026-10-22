using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryBagDescriptors")]
    public class BagDescriptors
    {
        public string BagDescriptorName { get; set; }
        public int BagCnt { get; set; }
        public IEnumerable<string> BagDescriptorDesc { get; set; }
        public string BagDescriptorSubCode { get; set; }

    }
}
