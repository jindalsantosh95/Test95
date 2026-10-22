namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class BaggagePolicy : SegmentPolicy
    {
        public BagProperties? depricated { get; set; }
        public FreeBag? FreeBags { get; set; }
        public IEnumerable<PaidBag>? PaidBags { get; set; }
    }
}