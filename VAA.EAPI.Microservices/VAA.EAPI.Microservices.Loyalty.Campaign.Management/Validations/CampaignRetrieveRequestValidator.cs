using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class CampaignRetrieveRequestValidator : AbstractValidator<CampaignDetailsRetrieveQuery>
    {
        public CampaignRetrieveRequestValidator()
        {
            RuleFor(x => x.CampaignId).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.CampaignCode));
            RuleFor(x => x.CampaignCode).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.CampaignId));
        }
    }
}
