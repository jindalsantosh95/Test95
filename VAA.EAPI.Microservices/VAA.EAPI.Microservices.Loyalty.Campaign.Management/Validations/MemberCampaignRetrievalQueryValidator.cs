using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class MemberCampaignRetrievalQueryValidator : AbstractValidator<MemberCampaignRetrievalQuery>
    {
        public MemberCampaignRetrievalQueryValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
