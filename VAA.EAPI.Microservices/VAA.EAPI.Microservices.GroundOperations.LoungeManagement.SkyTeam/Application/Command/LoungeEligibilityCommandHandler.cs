using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Application.Command
{
    public class LoungeEligibilityCommandHandler : IRequestHandler<LoungeEligibilityCommand, Response<LoungeEligibilityResponse>>
    {

        private readonly ISkyTeamEligibilityService _skyTeamEligibilityService;

        public LoungeEligibilityCommandHandler(ISkyTeamEligibilityService skyTeamEligibilityService)
        {
            _skyTeamEligibilityService = skyTeamEligibilityService ?? throw new ArgumentNullException(nameof(skyTeamEligibilityService));
        }

        public async Task<Response<LoungeEligibilityResponse>> Handle(LoungeEligibilityCommand request, CancellationToken cancellationToken)
        {   
            return await _skyTeamEligibilityService.Eligibility(request.LoungeEligibilityRequest);
        }
    }
}
