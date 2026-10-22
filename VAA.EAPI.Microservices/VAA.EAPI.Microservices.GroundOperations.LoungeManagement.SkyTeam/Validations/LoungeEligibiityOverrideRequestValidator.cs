using FluentValidation;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Validations
{
    public class LoungeEligibilityOverrideRequestValidator : AbstractValidator<LoungeEligibilityOverrideRequest>
    {
        public LoungeEligibilityOverrideRequestValidator() { }
    }
}
