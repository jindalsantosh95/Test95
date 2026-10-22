using VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo.FareRules;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo
{
    public class Detail
    {
        public IEnumerable<Category> Categories { get; set; }

        public static explicit operator Detail(OfferdFareRuleCategory[] source)
        {
            return new()
            {
                Categories = source.Select(category => (Category)category)
            };
        }
    }
}
