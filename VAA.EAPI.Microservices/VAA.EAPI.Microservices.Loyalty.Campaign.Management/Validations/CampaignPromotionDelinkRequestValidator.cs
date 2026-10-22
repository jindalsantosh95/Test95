using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionDelinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class CampaignPromotionDelinkRequestValidator : AbstractValidator<CampaignPromotionDelinkRequestData>
    {
        public CampaignPromotionDelinkRequestValidator()
        {
            RuleFor(x => x.CampaignId).NotNull().NotEmpty();
            RuleFor(x => x.PromotionId).NotNull().NotEmpty();
        }
    }
}
