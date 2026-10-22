using FluentValidation;
using System;
using System.Globalization;
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Notification.Constants;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Validations
{

    public class OrderNotificationRequestCommandValidator : AbstractValidator<OrderNotificationRequestCommand>
    {
        public OrderNotificationRequestCommandValidator()
        {
            RuleFor(x => x.NotificationRequest.OrderReference)
                .NotNull().WithMessage(ErrorCodes.invalidOrderReference);
            RuleFor(x => x.NotificationRequest.OrderReference)
                .NotEmpty().WithMessage(ErrorCodes.invalidOrderReference);

            RuleFor(x => x.NotificationRequest.CreationDate)
                .NotNull().WithMessage(ErrorCodes.invalidDateTimeFormat);
            RuleFor(x => x.NotificationRequest.CreationDate)
                .NotEmpty().WithMessage(ErrorCodes.invalidDateTimeFormat);
            RuleFor(x => x.NotificationRequest.CreationDate)
                .Must(date => IsValidDate(date, Constants.Application.OrderNotifyDateFormat)).WithMessage(ErrorCodes.invalidDateTimeFormat);

            RuleFor(x => x.NotificationRequest.CreationTime)
                .NotNull().WithMessage(ErrorCodes.invalidDateTimeFormat);
            RuleFor(x => x.NotificationRequest.CreationTime)
                .NotEmpty().WithMessage(ErrorCodes.invalidDateTimeFormat);
            RuleFor(x => x.NotificationRequest.CreationTime)
                .Must(dateTime => IsValidDateTime(dateTime, Constants.Application.OrderNotifyDateTimeISOFormat)).WithMessage(ErrorCodes.invalidDateTimeFormat);

            RuleFor(x => x.Channel)
                .NotNull().WithMessage(ErrorCodes.invalidChannel);
            RuleFor(x => x.Channel)
                .NotEmpty().WithMessage(ErrorCodes.invalidChannel);
            RuleFor(x => x.Channel)
                .Must(channel => Helper.ChannelValidator.ValdidateChannel(channel)).WithMessage(ErrorCodes.invalidChannel);

            RuleFor(x => x.NotificationRequest.FulfillmentDetails)
                .NotNull().WithMessage(ErrorCodes.invalidFulfillmentDetails);
            RuleFor(x => x.NotificationRequest.FulfillmentDetails)
                .NotEmpty().WithMessage(ErrorCodes.invalidFulfillmentDetails);
            RuleForEach(inputData => inputData.NotificationRequest.FulfillmentDetails)
            .SetValidator(inputData => new FulfillmentDetailsValidator(inputData.NotificationRequest));
        }
        public class FulfillmentDetailsValidator : AbstractValidator<FulfillmentDetails>
        {
            public FulfillmentDetailsValidator(OrderNotificationRequest orderNotificationRequest)
            {
                RuleFor(x => Convert.ToString(x.IdType))
                    .NotNull().WithMessage(ErrorCodes.invalidIdType);
                RuleFor(x => Convert.ToString(x.IdType))
                    .NotEmpty().WithMessage(ErrorCodes.invalidIdType);

                RuleFor(x => x.FulfillmentId)
                    .NotNull().WithMessage(ErrorCodes.invalidFulfillmentId);
                RuleFor(x => x.FulfillmentId)
                    .NotEmpty().WithMessage(ErrorCodes.invalidFulfillmentId);
            }
        }
        private static bool IsValidDate(string date, string orderNotifyDateFormat)
        {
            return DateTime.TryParseExact(date, orderNotifyDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
        private static bool IsValidDateTime(string date, string orderNotifyDateTimeFormat)
        {
            return DateTime.TryParseExact(date, orderNotifyDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

    }
}
