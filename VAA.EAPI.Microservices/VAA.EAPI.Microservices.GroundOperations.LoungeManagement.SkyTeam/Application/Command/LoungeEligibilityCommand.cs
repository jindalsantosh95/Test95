using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Application.Command
{
    public class LoungeEligibilityCommand : IRequest<Response<LoungeEligibilityResponse>>
    {
        public LoungeEligibilityRequest LoungeEligibilityRequest { get; set; }
    }
}
