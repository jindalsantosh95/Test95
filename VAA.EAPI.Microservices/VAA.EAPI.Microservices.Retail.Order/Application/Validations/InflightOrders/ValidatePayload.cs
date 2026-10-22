using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.InflightFetchOrders;
using VAA.EAPI.Microservices.Retail.Order.Constants;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Validations.InflightOrders
{
    public class ValidatePayload : IValidatePayload
    {
        public List<ErrorDetails> ValidateSearchOrderCommand(OrderSearchCommand orderSearchCommand, string guid)
        {
            List<ErrorDetails> validationErrors = new();

            #region Validate CustomerID
            if (string.IsNullOrEmpty(orderSearchCommand.CustomerId) || !Guid.TryParse(orderSearchCommand.CustomerId, out Guid guidOutput))
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidCustomerId), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            #endregion Validate CustomerID

            #region Validate FutureDaysInput
            int futureDays = 0;
            if (!int.TryParse(orderSearchCommand.FutureDays, out futureDays) || futureDays < 0)
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidfutureDays), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            #endregion Validate FutureDaysInput

            #region Validate PastDaysInput
            int pastDays = 0;
            if (!int.TryParse(orderSearchCommand.PastDays, out pastDays) || pastDays < 0)
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidPastDays), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            #endregion Validate PastDaysInput

            #region validate Loyalty customer status
            if (!string.IsNullOrEmpty(orderSearchCommand.MemberStatus) && orderSearchCommand.MemberStatus.ToUpper() != "ACTIVE")
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMemberStatus), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            return validationErrors;
            #endregion validate Loyalty customer status
        }
    }
}
