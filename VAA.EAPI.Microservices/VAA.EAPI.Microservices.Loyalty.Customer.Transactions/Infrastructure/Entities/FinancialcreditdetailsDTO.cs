using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class FinancialcreditdetailsDTO
    {
        public string Amount { get; set; }
        public string PartitionName { get; set; }
        public string PANNumber { get; set; }
        public string WFNA { get; set; }
        public string BonusMiles { get; set; }
        public string GiftMiles { get; set; }
        public string NonGiftAdjustments { get; set; }
        public string StandardMiles { get; set; }
        public string AwardIndicator { get; set; }
        public string SkipActivityStatement { get; set; }
        public string VAReward1Code { get; set; }
        public string VAReward2Code { get; set; }
        public string VAReward3Code { get; set; }
        public string VAReward4Code { get; set; }
        public string VAReward5Code { get; set; }
        public string VAReward6Code { get; set; }
        public string VAReward7Code { get; set; }

        public FinancialcreditdetailsDTO()
        {

        }

        public static explicit operator FinancialcreditdetailsDTO(Financialcreditdetails financialcreditdetails)
        {
            return new()
            {
                Amount = financialcreditdetails?.Amount,
                PartitionName = financialcreditdetails?.PartitionName,
                PANNumber = financialcreditdetails?.PANNumber,
                WFNA = financialcreditdetails?.WFNA,
                BonusMiles = financialcreditdetails?.BonusMiles,
                GiftMiles = financialcreditdetails?.GiftMiles,
                NonGiftAdjustments = financialcreditdetails?.NonGiftAdjustments,
                StandardMiles = financialcreditdetails?.StandardMiles,
                AwardIndicator = financialcreditdetails?.AwardIndicator,
                SkipActivityStatement = financialcreditdetails?.SkipActivityStatement,
                VAReward1Code = financialcreditdetails?.VAReward1Code,
                VAReward2Code = financialcreditdetails?.VAReward2Code,
                VAReward3Code = financialcreditdetails?.VAReward3Code,
                VAReward4Code = financialcreditdetails?.VAReward4Code,
                VAReward5Code = financialcreditdetails?.VAReward5Code,
                VAReward6Code = financialcreditdetails?.VAReward6Code,
                VAReward7Code = financialcreditdetails?.VAReward7Code
            };
        }
    }
}
