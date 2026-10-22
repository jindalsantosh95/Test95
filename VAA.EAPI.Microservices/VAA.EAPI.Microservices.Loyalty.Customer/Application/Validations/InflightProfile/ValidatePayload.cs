using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.InflightProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Validations.InflightProfile
{
    public static class ValidatePayload
    {
        public static List<ErrorDetails> Validate(CustomerProfileCommand customerProfileQuery, string guid)
        {
            List<ErrorDetails> validationErrors = new List<ErrorDetails>();

            #region Validate CustomerID
            if (string.IsNullOrEmpty(customerProfileQuery.CustomerId) || !Guid.TryParse(customerProfileQuery.CustomerId, out Guid guidOutput))
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), guid, Constants.ErrorCodes.InflightExceptionFilePath);
            }
            #endregion Validate CustomerID

            #region validate Loyalty customer status
            //if (!string.IsNullOrEmpty(customerProfileQuery.MemberStatus) && customerProfileQuery.MemberStatus.ToUpper() != "ACTIVE")
            //{
            //    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidMemberStatus), guid, Constants.ErrorCodes.InflightExceptionFilePath);
            //}
            return validationErrors;
            #endregion validate Loyalty customer status
        }
    }
}
