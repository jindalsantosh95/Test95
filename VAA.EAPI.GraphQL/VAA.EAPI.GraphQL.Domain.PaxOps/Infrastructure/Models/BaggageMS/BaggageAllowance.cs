namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class BaggageAllowance
    {
        public string Title { get; set; }
        public IEnumerable<string> Description { get; set; }
        public IconType? IconType { get; set; }
    }
}