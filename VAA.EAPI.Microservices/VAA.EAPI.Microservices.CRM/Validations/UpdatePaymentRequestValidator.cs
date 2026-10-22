using FluentValidation;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Domain;

namespace VAA.EAPI.Microservices.CRM.Validations
{
    public class UpdatePaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public UpdatePaymentRequestValidator()
        {
            if (ConfigReader.Get<bool>(Constants.Environment.CRMWebApiResponsiveFeature))
            {
                RuleFor(x => x.PaymentNumber)
                        .NotNull().WithMessage(Constants.ErrorCodes.InvalidPaymentNumber);
                RuleFor(x => x.PaymentNumber)
                    .NotEmpty().WithMessage(Constants.ErrorCodes.InvalidPaymentNumber);
            }

            RuleFor(x => x.BankCountryName)
                .NotNull().WithMessage(Constants.ErrorCodes.InvalidBankCountryName);
            RuleFor(x => x.BankCountryName)
                .NotEmpty().WithMessage(Constants.ErrorCodes.InvalidBankCountryName);

            RuleFor(x => x.BankName)
                .NotNull().WithMessage(Constants.ErrorCodes.MissingInvalidBankName);
            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage(Constants.ErrorCodes.MissingInvalidBankName);

            RuleFor(x => x.AccountHolderName)
                .NotNull().WithMessage(Constants.ErrorCodes.MissingAccountHolderName);
            RuleFor(x => x.AccountHolderName)
                .NotEmpty().WithMessage(Constants.ErrorCodes.MissingAccountHolderName);
        }
    }
}
