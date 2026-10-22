using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class CampaignPromotionLinkRequestValidator : AbstractValidator<CampaignPromotionLinkRequestData>
    {
        public CampaignPromotionLinkRequestValidator()
        {
            RuleFor(x => x.CampaignId).NotNull().NotEmpty();
            RuleFor(x => x.PromotionId).NotNull().NotEmpty();
            RuleFor(x => x.CreatedBy).NotNull().NotEmpty();
        }
    }
}
