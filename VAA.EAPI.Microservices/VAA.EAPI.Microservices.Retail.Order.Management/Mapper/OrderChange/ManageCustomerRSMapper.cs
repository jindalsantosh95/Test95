using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class ManageCustomerRSMapper
    {
        public static OrderView ResponseMapper(List<Warning> warnings, ManageCustomerRequest request)
        {
            if (warnings.Count == 0)
            {
                return new OrderView()
                {
                    BookingReferences = new()
                    {
                        RLOC = request.Rloc
                    },
                    OrderNumber = new()
                    {
                        OrderId = request.OrderId
                    }
                };
            }
            else if (warnings.Count < request.CustomerDetails.Count)
            {
                return new OrderView()
                {
                    BookingReferences = new()
                    {
                        RLOC = request.Rloc
                    },
                    OrderNumber = new()
                    {
                        OrderId = request.OrderId,
                    },
                    Warnings = warnings
                };
            }
            else
                return null;
        }

        public static Warning WarningMapper(Customer customer, string errorMessage)
        {
            switch (errorMessage)
            {
                case Constants.Application.InvalidDOBMessage:
                    return new Warning() { Code = ErrorCodes.InvalidDOBCode, Message = $"Failed to add the details for {customer.CustId}" };
                case Constants.Application.InvalidGenderMessage:
                    return new Warning() { Code = ErrorCodes.InvalidGenderCode, Message = $"Failed to add the details for {customer.CustId}" };
                default:
                    return new Warning() { Code = ErrorCodes.DeltaManageMultipleDocError, Message = $"Failed to add the details for {customer.CustId}" };
            }
        }
    }
}
