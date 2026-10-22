using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation;
using VAA.EAPI.Microservices.Retail.Order.Constants;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Validations.InflightOrders
{
    public class ValidatePayloadAssociateFlights : IValidatePayloadAssociateFlights
    {
        public List<ErrorDetails> ValidateOrderAssociationRequestCommand(OrderAssociationRequest orderAssociationRequest, string guid)
        {
            List<ErrorDetails> validationErrors = new();

            #region Validate CustomerID
            if (string.IsNullOrEmpty(orderAssociationRequest.CustomerId) || !Guid.TryParse(orderAssociationRequest.CustomerId, out Guid guidOutput))
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidCustomerId), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            #endregion Validate CustomerID

            #region Validate First Name
            if (!Regex.Match(orderAssociationRequest.FirstName, "^[a-zA-Z]*$").Success || string.IsNullOrWhiteSpace(orderAssociationRequest.FirstName))
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidFirstName), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            #endregion Validate First Name

            #region Validate Last Name
            if (!Regex.Match(orderAssociationRequest.LastName, "^[a-zA-Z]*$").Success || string.IsNullOrWhiteSpace(orderAssociationRequest.LastName))
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidLastName), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            #endregion Validate Last Name

            #region Validate Booking reference number
            if (!Regex.Match(orderAssociationRequest.PNRNumber, "^[A-Z0-9]{6}$").Success || string.IsNullOrWhiteSpace(orderAssociationRequest.PNRNumber))
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidPNR), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            #endregion Validate Booking reference number

            #region validate Loyalty customer status
            if (!string.IsNullOrEmpty(orderAssociationRequest.MemberStatus) && orderAssociationRequest.MemberStatus.ToUpper() != "ACTIVE")
            {
                return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMemberStatus), guid, Constants.Application.InflightOrdersExceptionFilePath);
            }
            return validationErrors;
            #endregion validate Loyalty customer status
        }
    }
}
