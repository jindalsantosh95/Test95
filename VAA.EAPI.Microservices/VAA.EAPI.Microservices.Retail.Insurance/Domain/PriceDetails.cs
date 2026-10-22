namespace VAA.EAPI.Microservices.Retail.Insurance.Domain
{
    public class PriceDetails
    {
        public bool IsAvailable { get; set; }
        public bool IsIncluded { get; set; }
        public double Price { get; set; }
        public IEnumerable<Sequence>? Sequences { get; set; }
    }
}