using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class RollbackVoucherRdmValidator : AbstractValidator<RollbackVoucherRdmRequest>
    {
        public RollbackVoucherRdmValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.VoucherNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.RollbackInitiatedBy).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.RollbackSource).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
