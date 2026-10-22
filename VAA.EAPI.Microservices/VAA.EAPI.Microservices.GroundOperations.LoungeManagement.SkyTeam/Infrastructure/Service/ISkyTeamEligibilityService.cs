using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure
{
    public interface ISkyTeamEligibilityService
    {
        public Task<Response<LoungeEligibilityResponse>> Eligibility(LoungeEligibilityRequest request);
        public Task<Response<LoungeEligibilityOverrideResponse>> EligibilityOverride(LoungeEligibilityOverrideRequest request);
    }
}
