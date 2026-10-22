using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Validations
{
    public class SupplementarySilverAccountLinkCommandValidator : AbstractValidator<SupplementarySilverAccountLinkCommand>
    {
        public SupplementarySilverAccountLinkCommandValidator()
        {
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.HeadCustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.HeadMemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.ChildCustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.ChildMemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}