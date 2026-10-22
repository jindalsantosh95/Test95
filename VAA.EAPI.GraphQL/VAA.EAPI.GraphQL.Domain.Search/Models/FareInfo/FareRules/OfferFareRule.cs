namespace VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo.FareRules
{
    public class OfferFareRule
    {
        public IEnumerable<OfferdFareRuleCategory> OfferdFareRuleCategoryList { get; set; }
        public string FareBasisCode { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
    }
}
