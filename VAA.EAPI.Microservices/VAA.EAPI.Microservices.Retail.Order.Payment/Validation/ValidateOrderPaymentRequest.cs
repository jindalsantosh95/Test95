using FluentValidation;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Validation
{
    public class ValidateOrderPaymentRequest : AbstractValidator<OrderPaymentEventCommand>
    {
        public ValidateOrderPaymentRequest()
        {
            RuleFor(x => x.OrderReference).NotEmpty().WithMessage(ErrorCodes.NullOrEmptyOrderReference);

            RuleFor(x => x.Channel).NotEmpty().WithMessage(ErrorCodes.InvalidOrNullOrEmptyChannel);
            RuleFor(x => x.Channel).NotNull().WithMessage(ErrorCodes.InvalidOrNullOrEmptyChannel);
            RuleFor(x => x.Channel)
                .Must((command, enumMemberValue) => PaymentChannel.IsValid(command.Channel))
                .WithMessage(ErrorCodes.InvalidOrNullOrEmptyChannel);

            RuleFor(x => x.PaymentRequest)
                .NotNull()
                .WithMessage(ErrorCodes.InvalidOrNullOrEmptyPaymentInfo);

            bool.TryParse(ConfigReader.Get(Constants.Environment.EnableMultiplePayment), out bool enableMultiplePayment);
            if (!enableMultiplePayment)
                RuleFor(x => x.PaymentRequest)
                .Must((command, z) => command.PaymentRequest.Count.Equals(1))
                .WithMessage(ErrorCodes.InvalidOrNullOrEmptyPaymentInfo);

            RuleForEach(x => x.PaymentRequest).SetValidator(new OrderPaymentRequestValidator());
        }


        public class OrderPaymentRequestValidator : AbstractValidator<OrderPaymentRequest>
        {
            public OrderPaymentRequestValidator()
            {
                RuleFor(s => s.TransactionId).NotEmpty().WithMessage(ErrorCodes.NullOrEmptyTransactionId);

                RuleFor(s => s.TransactionDateTime).NotEmpty().WithMessage(ErrorCodes.InvalidOrNullOrEmptyTransactionDateTime);
                RuleFor(s => s.TransactionDateTime)
                    .Must((command, enumMemberValue) => IsValidDateTime(command.TransactionDateTime, Constants.Application.TransactionDateTimeIsoFormat))
                    .WithMessage(ErrorCodes.InvalidOrNullOrEmptyTransactionDateTime);

                RuleFor(s => s.TransactionAmount)
                    .NotNull()
                    .GreaterThanOrEqualTo(0)
                    .WithMessage(ErrorCodes.NullOrEmptyTransactionAmount);

                RuleFor(s => s.PaymentMethod).NotEmpty().WithMessage(ErrorCodes.InvalidOrNullOrEmptyPaymentMethod);
                RuleFor(s => s.PaymentType).NotEmpty().WithMessage(ErrorCodes.InvalidOrNullOrEmptyPaymentType);


                RuleFor(s => s.PaymentSubType).NotEmpty()
                    .WithMessage(ErrorCodes.InvalidOrNullOrEmptyPaymentSubType)
                    .When(s => s.PaymentMethod.Equals(PaymentMethod.CD));

                RuleFor(s => s.CardName).NotEmpty()
                    .WithMessage(ErrorCodes.InvalidOrNullOrEmptyCardName)
                    .When(s => s.PaymentMethod.Equals(PaymentMethod.CD));

                RuleFor(s => s.CardNumber).NotEmpty()
                    .WithMessage(ErrorCodes.InvalidOrNullOrEmptyCardNumber)
                    .When(s => s.PaymentMethod.Equals(PaymentMethod.CD));

                RuleFor(s => s.CardExpiry).NotEmpty()
                    .WithMessage(ErrorCodes.InvalidOrNullOrEmptyCardExpiry)
                    .Must((command, enumMemberValue) => IsValidCardExpiry(command.CardExpiry))
                    .WithMessage(ErrorCodes.InvalidOrNullOrEmptyCardExpiry)
                    .When(s => s.PaymentMethod.Equals(PaymentMethod.CD));


                RuleFor(s => s.Currency).NotEmpty().WithMessage(ErrorCodes.InvalidOrNullOrEmptyPaymentCurrency);
                RuleFor(s => s.AuthorisationCode).NotEmpty().WithMessage(ErrorCodes.InvalidOrNullOrEmptyPaymentAuthCode);
                RuleFor(s => s.PspName).NotEmpty().WithMessage(ErrorCodes.InvalidOrNullOrEmptyPSPName);
                RuleFor(s => s.PspId).NotEmpty().WithMessage(ErrorCodes.InvalidOrNullOrEmptyPSPId);
            }

            private static bool IsValidDateTime(string date, string transactionDateTimeFormat)
            {
                return DateTime.TryParseExact(date, transactionDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
            }

            private static bool IsValidCardExpiry(string value)
            {
                string pattern = Constants.Application.CardExpiryPattern;
                var rg = new Regex(pattern);
                return rg.IsMatch(value);
            }

        }
    }
}
