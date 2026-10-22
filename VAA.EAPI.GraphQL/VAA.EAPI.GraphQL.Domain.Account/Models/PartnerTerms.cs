namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class PartnerTerms
    {
        public int RetroClaimPeriod { get; set; }
        public DateTime AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public int RetroClaimMinimumPeriod { get; set; }
        public string PartnerCode { get; set; }
    }
}
