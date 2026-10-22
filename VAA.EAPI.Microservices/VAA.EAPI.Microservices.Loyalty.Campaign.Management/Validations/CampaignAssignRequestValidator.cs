using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class CampaignAssignRequestValidator : AbstractValidator<CamapignAssignRequestData>
    {
        public CampaignAssignRequestValidator()
        {
            RuleFor(m => m.CustomerId).NotEmpty().When(m => string.IsNullOrEmpty(m.MemberNumber));
            RuleFor(m => m.MemberNumber).NotEmpty().When(m => string.IsNullOrEmpty(m.CustomerId));
            RuleFor(x => x.CampaignCode).NotNull().NotEmpty();
            RuleFor(x => x.CampaignId).NotNull().NotEmpty();
            RuleFor(x => x.CreatedBy).NotNull().NotEmpty();
        }
    }
}
