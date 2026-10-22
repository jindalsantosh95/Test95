using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class PartnerMembershipCreateValidator : AbstractValidator<PartnerMembership>
    {
        public PartnerMembershipCreateValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.CustomerIdNotFoundMessage);
            RuleFor(x => x.PartnerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.StartDate).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.PartnerStatusCode).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
