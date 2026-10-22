using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Application.Command
{
    public class LoungeEligibilityOverrideCommand : IRequest<Response<LoungeEligibilityOverrideResponse>>
    {
        public LoungeEligibilityOverrideRequest LoungeEligibilityOverrideRequest { get; set; }
    }
}
