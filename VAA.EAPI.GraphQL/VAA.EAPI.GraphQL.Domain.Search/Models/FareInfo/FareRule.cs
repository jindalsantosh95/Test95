namespace VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo
{
    public class FareRule
    {
        public IEnumerable<FareRuleSegment> Segments { get; set; }
        public Detail Detail { get; set; }
        public string FareBasisCode { get; set; }
    }
}
