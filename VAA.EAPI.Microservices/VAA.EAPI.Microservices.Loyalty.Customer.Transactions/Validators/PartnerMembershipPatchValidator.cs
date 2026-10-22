using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class PartnerMembershipPatchValidator : AbstractValidator<PartnerMembership>
    {
        public PartnerMembershipPatchValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.CustomerIdNotFoundMessage);
            RuleFor(x => x.EndDate).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}