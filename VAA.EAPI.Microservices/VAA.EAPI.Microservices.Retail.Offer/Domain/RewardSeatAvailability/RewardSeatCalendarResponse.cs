using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability
{

    public sealed class RewardSeatCalendarResponse
    {
        public required IEnumerable<RewardSeatCalendarCandidate> Candidates { get; init; }
        public required string MarketPolicyVersion { get; init; }
        public required string FareFamilyMappingVersion { get; init; }
        public required bool Complete { get; init; }
    }
}
