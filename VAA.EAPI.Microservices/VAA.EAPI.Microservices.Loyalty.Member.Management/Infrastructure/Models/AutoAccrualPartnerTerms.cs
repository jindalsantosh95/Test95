using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models
{
    public class AutoAccrualPartnerTerms
    {
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public int? RetroClaimPeriod { get; set; }
    }
}
