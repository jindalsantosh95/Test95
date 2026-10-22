using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class CampaignUnassignRequestValidator : AbstractValidator<CampaignUnassignRequestData>
    {
        public CampaignUnassignRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotNull().NotEmpty()
                .WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.DocumentId)
                .NotNull().NotEmpty()
                .WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}

