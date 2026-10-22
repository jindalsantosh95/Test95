using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class PatchVoucherRelatedTxnsPnrValidator : AbstractValidator<PatchVoucherRelatedTxnsPnrRequest>
    {
        public PatchVoucherRelatedTxnsPnrValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.VoucherNumbers).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.PNR).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
