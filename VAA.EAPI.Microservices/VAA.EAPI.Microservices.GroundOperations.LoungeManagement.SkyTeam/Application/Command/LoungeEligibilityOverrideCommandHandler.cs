using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Application.Command
{
    public class LoungeEligibilityOverrideCommandHandler : IRequestHandler<LoungeEligibilityOverrideCommand, Response<LoungeEligibilityOverrideResponse>>
    {

        private readonly ISkyTeamEligibilityService _skyTeamEligibilityService;

        public LoungeEligibilityOverrideCommandHandler(ISkyTeamEligibilityService skyTeamEligibilityService)
        {
            _skyTeamEligibilityService = skyTeamEligibilityService ?? throw new ArgumentNullException(nameof(skyTeamEligibilityService));
        }

        public async Task<Response<LoungeEligibilityOverrideResponse>> Handle(LoungeEligibilityOverrideCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await _skyTeamEligibilityService.EligibilityOverride(request.LoungeEligibilityOverrideRequest);
            }
            catch (Exception ex)
            {
                throw (new ApiFailedException("SkyTeam-FrontEnd-EligibilityOverride", new Exception(ex.Message)));
            }
        }
    }
}

