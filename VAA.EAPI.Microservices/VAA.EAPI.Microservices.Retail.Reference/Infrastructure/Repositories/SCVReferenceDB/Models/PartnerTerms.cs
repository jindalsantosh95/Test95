using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models
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
