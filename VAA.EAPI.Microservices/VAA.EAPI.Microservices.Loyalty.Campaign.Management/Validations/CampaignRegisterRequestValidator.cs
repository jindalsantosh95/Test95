using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class CampaignRegisterRequestValidator : AbstractValidator<CamapignRegisterRequestData>
    {
        public CampaignRegisterRequestValidator()
        {
            RuleFor(x => x.CampaignCode).NotNull().NotEmpty();
            RuleFor(x => x.CampaignId).NotNull().NotEmpty();
            RuleFor(x => x.DocumentId).NotNull().NotEmpty();
            RuleFor(x => x.CustomerId).NotNull().NotEmpty();
        }
    }
}