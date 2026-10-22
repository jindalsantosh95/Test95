using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement
{
    public class VSRetroClaimResponse
    {
        public int TotalPointsEarned { get; set; }

        public int TotalTierPointsEarned { get; set; }

        public IEnumerable<VSClaimDetails> Flights { get; set; }
    }
}

