using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class PromotionCampaignSearchRequestValidator : AbstractValidator<PromotionCampaignSearchRequestData>
    {
        public PromotionCampaignSearchRequestValidator()
        {
            RuleFor(x => x.PromotionId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
