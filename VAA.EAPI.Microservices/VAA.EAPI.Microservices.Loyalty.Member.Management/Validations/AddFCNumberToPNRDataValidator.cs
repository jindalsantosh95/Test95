using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Validations
{
    public class AddFCNumberToPNRDataValidator : AbstractValidator<MemberDetailsToAddSSR>
    {
        public AddFCNumberToPNRDataValidator()
        {
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.PNR).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.FirstName).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.LastName).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.Pos).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.OrderId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }

    }
}
