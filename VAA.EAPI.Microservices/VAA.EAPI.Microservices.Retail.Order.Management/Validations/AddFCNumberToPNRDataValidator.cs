using FluentValidation;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.StampFCNumberToPNR;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Validations
{
    public class AddFCNumberToPNRDataValidator : AbstractValidator<FCStampingDetails>
    {
        public AddFCNumberToPNRDataValidator()
        {
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.PNR).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.FirstName).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.LastName).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
