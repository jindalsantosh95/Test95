
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class MemberCustomAttributes
    {
        public string ActiveYearCounterForTenureBenefits { get; set; }
        public string TotalActiveYear { get; set; }
        public IEnumerable<MemberLastFiveVSActivityYear> MemberLastFiveVSActivityYear { get; set; }
    }
}
