using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public sealed class VoucherMonitoringRequestValidator : AbstractValidator<VoucherMonitoringRequest>
    {
        public VoucherMonitoringRequestValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.VoucherNumbers).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}

