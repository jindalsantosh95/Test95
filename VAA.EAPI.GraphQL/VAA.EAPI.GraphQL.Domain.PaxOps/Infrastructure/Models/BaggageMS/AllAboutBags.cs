namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class AllAboutBags
    {
        public IEnumerable<BaggageAllowance> EveryoneGets { get; set; }
        public IEnumerable<BaggageAllowance> ByCabin { get; set; }
        public string BaggageUrl { get; set; }
    }
}