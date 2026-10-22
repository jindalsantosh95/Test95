using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Order.Validations.Common
{
    public class AgencyValidation
    {
        public static Response<T> ValidateAgencyDetails<T>(ValidationRS validationRS, IHostEnvironment hostEnvironment)
        {
            var errors = validationRS.Error.Select(error =>
            {
                if (error?.Code == "G009")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_22"
                    };
                }
                else if (error?.Code == "10144")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_2"
                    };
                }
                else if (error?.Code == "10146")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_3"
                    };
                }
                else if (error?.Code == "10147")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_4"
                    };
                }
                else if (error?.Code == "10145")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_5"
                    };
                }
                else if (error?.Code == "10140")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_6"
                    };
                }
                else if (error?.Code == "10152")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_8"
                    };
                }
                else if (error?.Code == "10153")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_45"
                    };
                }
                else
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_22"
                    };
                }
            });

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }
        public static Response<T> ValidateEasyPayFOP<T>(ValidationRS validationRS, IHostEnvironment hostEnvironment, bool isEasyPay)
        {
            var errors = new List<ErrorDetail>();

            var agencyPermission = validationRS?.ValidateResponse?.AgencyPermissions;
            bool isPaymentAllowed = agencyPermission?.BspActiveStatusInd == true && agencyPermission?.AirlineTicketingAuthorityInd == true && agencyPermission?.EasyPayFopAllowedInd == true;

            if (!isEasyPay || !isPaymentAllowed)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "368_9"
                });
            }

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateFOPCash<T>(IATA_OrderCreateRQ orderCreateRQ, ValidationRS validationRS, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            var paymentMethodType = orderCreateRQ.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod;
            var paymentTypeCode = ((VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.SettlementPlanType)paymentMethodType.Item).PaymentTypeCode;
            var agencyPermission = validationRS?.ValidateResponse?.AgencyPermissions;
            bool isPaymentAllowed = agencyPermission?.BspActiveStatusInd == true && agencyPermission?.AirlineTicketingAuthorityInd == true && agencyPermission?.CashFopAllowedInd == true;

            if (paymentTypeCode != null && paymentTypeCode.ToUpper() != "CA" || !isPaymentAllowed)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "368_8"
                });
            }

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateOrderChangeFOPCash<T>(IATA_OrderChangeRQ orderChangeRQ, ValidationRS validationRS, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            var paymentRq = orderChangeRQ.Request.PaymentFunctions?.First().PaymentProcessingDetails;
            var paymentTypeCode = (paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.SettlementPlanType)?.PaymentTypeCode;

            var agencyPermission = validationRS.ValidateResponse.AgencyPermissions;

            bool isPaymentAllowed = agencyPermission.BspActiveStatusInd == true && agencyPermission.AirlineTicketingAuthorityInd == true && agencyPermission.CashFopAllowedInd == true;

            if (paymentTypeCode != null && paymentTypeCode != "CA" || !isPaymentAllowed)
            {
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "368_9" });
            }
            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateEasyFop<T>(bool isEasyFop, ValidationRS validationRS, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            var agencyPermission = validationRS.ValidateResponse.AgencyPermissions;

            bool isEasyFoAllowed = agencyPermission.BspActiveStatusInd == true && agencyPermission.AirlineTicketingAuthorityInd == true && agencyPermission.EasyPayFopAllowedInd == true;

            if (!isEasyFop || !isEasyFoAllowed)
            {
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "368_9" });
            }
            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateArcAgentStatus<T>(CheckResponse checkResponse, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            bool isArcTicketingAllowed = checkResponse.TransactionResponse.Arc.TicketingStatus == Constants.Application.TicketingOn;

            bool isAirlineTicketingAllowed = checkResponse.TransactionResponse.Ticketing.First().Airline.TicketingStatus == Constants.Application.TicketingOn;

            if (!isArcTicketingAllowed || !isAirlineTicketingAllowed)
            {
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "368_8" });
            }

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }
    }
}
