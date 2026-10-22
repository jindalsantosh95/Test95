using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class RedeemVoucherValidator : AbstractValidator<RedeemVoucherRequest>
    {
        public RedeemVoucherValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.FlightDetails).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.PartnerCode).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.VoucherDetails).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.FlightDetails.ActivityDate).NotNull().NotEmpty().When(x => x.FlightDetails != null).WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.VoucherDetails.VoucherNumber).NotNull().NotEmpty().When(x => x.VoucherDetails != null).WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.VoucherDetails.PointsToAward).NotNull().NotEmpty().When(x => x.VoucherDetails != null).WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.RedemptionType).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.TransactionChannel).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.FlightDetails.OriginAirport).NotNull().NotEmpty().When(x => x.FlightDetails != null).WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.FlightDetails.DestinationAirport).NotNull().NotEmpty().When(x => x.FlightDetails != null).WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
