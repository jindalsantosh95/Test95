namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class BagDescriptors
    {
        public string BagDescriptorName { get; set; }
        public int BagCnt { get; set; }
        public IEnumerable<string> BagDescriptorDesc { get; set; }
        public string BagDescriptorSubCode { get; set; }

        public static explicit operator BagDescriptors(FlightOffer.BagDescriptor source)
        {
            return new()
            {
                BagCnt = (int)source.BagCnt,
                BagDescriptorDesc = source.BagDescriptorDesc,
                BagDescriptorName = source.BagDescriptorName,
                BagDescriptorSubCode = source.BagDescriptorSubCode
            };
        }
    }
}
