using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility;

namespace VAA.EAPI.LoungeManagement.Clues.Application.Command
{
    public class LoungeEligibilityCommand : IRequest<Response<LoungeEligibilityResponse>>
    {
        public LoungeEligibilityRequest LoungeEligibilityRequest { get; set; }
    }
}
