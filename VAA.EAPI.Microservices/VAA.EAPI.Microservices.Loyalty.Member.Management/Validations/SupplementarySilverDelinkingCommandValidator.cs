using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Validations
{
    public class SupplementarySilverDelinkingCommandValidator : AbstractValidator<SupplementarySilverDelinkingCommand>
    {
        public SupplementarySilverDelinkingCommandValidator()
        {
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.HeadAccount).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.HeadAccount.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.HeadAccount.MemberId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);

            RuleFor(x => x.ChildAccount).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.ChildAccount.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.ChildAccount.MemberId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
