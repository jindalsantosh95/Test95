using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class MemberCustomAttributesAccount
    {
        public string ActiveYearCounterForTenureBenefits { get; set; }
        public string TotalActiveYear { get; set; }
        public IEnumerable<MemberLastFiveVSActivityYearAccount> MemberLastFiveVSActivityYear { get; set; }
        public bool IsAnActiveYear { get; set; }
    }
}
