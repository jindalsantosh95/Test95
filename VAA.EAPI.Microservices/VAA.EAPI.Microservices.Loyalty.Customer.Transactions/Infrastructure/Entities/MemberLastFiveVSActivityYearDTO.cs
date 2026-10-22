using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class MemberLastFiveVSActivityYearDTO
    {
        public string ActivityYear { get; set; }
        public string BenefitGiven { get; set; }

        public static explicit operator MemberLastFiveVSActivityYearDTO(MemberLastFiveVSActivityYear activityYear)
        {
            return new()
            {
                ActivityYear = activityYear.ActivityYear,
                BenefitGiven = activityYear.BenefitGiven,
            };
        }
    }
}
