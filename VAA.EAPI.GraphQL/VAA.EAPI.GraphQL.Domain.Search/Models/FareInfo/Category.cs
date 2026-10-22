using VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo.FareRules;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo
{
    public class Category
    {
        public string category { get; set; }
        public IEnumerable<string> text { get; set; }

        public static explicit operator Category(OfferdFareRuleCategory source)
        {
            return new()
            {
                category = source.OfferRuleCateogryText,
                text = new string[] { source.OfferRuleCateogryDesc }
            };
        }
    }
}