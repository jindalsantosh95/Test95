using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Validations
{
    public class OrderChangeRequestValidator
    {
        public static Result<Response<string>> ValidateRequest(OrderChangeRequestData orderChangeRequestData)
        {
            var errorList = new List<ErrorDetails>();
            var response = new Response<string>()
            {
                Status = Status.OK,
                Errors = null
            };

            //Skip validation for Post sale seat flow.
            if (orderChangeRequestData.OrderAmendments.Any(x => x.ProductType.ToString() == Enum.GetName(OrderAmendment.ProductTypes.SeatEnum)))
                return response;

            //validating customerDetails
            if (orderChangeRequestData.CustomerDetails == null || !orderChangeRequestData.CustomerDetails.Any())
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MissingCustomerDetailsErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validating orderAmendments
            if (orderChangeRequestData.OrderAmendments == null || !orderChangeRequestData.OrderAmendments.Any())
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MissingOrderAmendmentsErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validating flightItems
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems == null || !x.FlightItems.Any()))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MissingFlightItemsErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validating customerReference
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.CustomerReference == null || !x.CustomerReference.Any()))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MissingCustomerReferenceErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate customerId
            if (orderChangeRequestData.CustomerDetails != null && orderChangeRequestData.CustomerDetails.Any(x => string.IsNullOrEmpty(Convert.ToString(x.CustId))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.CustomerIDMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate FirstName
            if (orderChangeRequestData.CustomerDetails != null && orderChangeRequestData.CustomerDetails.Any(x => string.IsNullOrEmpty(x.FirstName)))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.FirstNameMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate LastName
            if (orderChangeRequestData.CustomerDetails != null && orderChangeRequestData.CustomerDetails.Any(x => string.IsNullOrEmpty(x.LastName)))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.LastNameMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate PtcLength
            if (orderChangeRequestData.CustomerDetails != null && (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.CustomerDetails.Any(x => string.IsNullOrEmpty(x.Ptc)) || orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.CustomerDetails.Any(x => x.Ptc.Length != ConfigReader.Get<int>(Constants.Environment.PtcLength))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidPtcLength), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate OriginDestId
            if (orderChangeRequestData.OrderAmendments != null && (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OriginDestinations.Any(x => string.IsNullOrEmpty(x.OriginDestId)) || orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.Any(y => string.IsNullOrEmpty(y.Segments.OriginDestId)))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidOriginDestId), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate OriginAirportCode
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OriginDestinations.Any(x => string.IsNullOrEmpty(x?.OriginAirportCode) || x?.OriginAirportCode.Length != ConfigReader.Get<int>(Constants.Environment.OriginAirportCodeLength)) || orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.Any(y => string.IsNullOrEmpty(y.Segments.OriginAirportCode) || y.Segments.OriginAirportCode.Length != ConfigReader.Get<int>(Constants.Environment.OriginAirportCodeLength))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidOriginAirportCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate DestinationAirportCode
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OriginDestinations.Any(x => string.IsNullOrEmpty(x?.DestinationAirportCode) || x?.DestinationAirportCode.Length != ConfigReader.Get<int>(Constants.Environment.DestAirportCodeLength)) || orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.Any(y => string.IsNullOrEmpty(y.Segments.DestinationAirportCode) || y.Segments.DestinationAirportCode.Length != ConfigReader.Get<int>(Constants.Environment.DestAirportCodeLength))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidDestinationAirportCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate productType is blank
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => string.IsNullOrEmpty(Convert.ToString(x.ProductType))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidProductType), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate Action Context is blank
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.Any(y => string.IsNullOrEmpty(Convert.ToString(y.ActionContext)))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.ActionContextMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate SegmentId 
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.Any(y => string.IsNullOrEmpty(y.Segments.SegmentId) || y.Segments.SegmentId.Length != ConfigReader.Get<int>(Constants.Environment.SegmentIdLength))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidSegmentId), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            // validate MarketingCarrier length
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.Any(y => string.IsNullOrEmpty(y.Segments?.MarketingCarrier) || y.Segments?.MarketingCarrier.Length != ConfigReader.Get<int>(Constants.Environment.MarketingCarrierLength))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMarketingCarrier), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            // validate MarketingFlightNumber
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.Any(y => string.IsNullOrEmpty(y.Segments?.MarketingFlightNumber) || !(ConfigReader.Get<int>(Constants.Environment.MarketingFlightNumberMinLength) <= y.Segments?.MarketingFlightNumber.Length && y.Segments.MarketingFlightNumber.Length <= ConfigReader.Get<int>(Constants.Environment.MarketingFlightNumberMaxLength)))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMarketingFlightNumber), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            // validate ClassOfService
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.Any(y => string.IsNullOrEmpty(y.Segments.ClassOfService) || y.Segments.ClassOfService.Length != ConfigReader.Get<int>(Constants.Environment.ClassOfServiceLength))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.ClassOfServiceMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            // validate OfferRef Id
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.OrderByReference == null || x.OrderByReference != null && x.OrderByReference.Any(y => string.IsNullOrEmpty(y.ItemsForAddition.OfferRefID))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MissingOrderRefId), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //Validating PNR length should be 6 characters
            if (orderChangeRequestData.Rloc == null || orderChangeRequestData.Rloc.Trim().Length != Constants.Application.PNRLength)
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidAttributeLenght), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //Validating proper Email format in customer details
            if (orderChangeRequestData.CustomerDetails != null && orderChangeRequestData.CustomerDetails.Any())
            {
                CustomerDetails customer = orderChangeRequestData.CustomerDetails.First();
                if (string.IsNullOrEmpty(customer.Contacts.Email) || !IsValidEmailId(customer.Contacts.Email.Trim()))
                {
                    errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidEmailFromat), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                    response = new Response<string>()
                    {
                        Status = Status.BusinessException,
                        Errors = errorList
                    };
                }

            }

            //Validating proper Datetime format for relevant fields
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null))
            {
                IEnumerable<Segment> segments = from orderamendment in orderChangeRequestData.OrderAmendments
                                                from flightitem in orderamendment.FlightItems
                                                select flightitem.Segments;
                foreach (Segment segment in segments)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(segment.ScheduledDepartureTime)) || !IsValidDate(Convert.ToString(segment.ScheduledDepartureTime)))
                    {
                        errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.SchDepTimeFormat), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                        response = new Response<string>()
                        {
                            Status = Status.BusinessException,
                            Errors = errorList
                        };
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(segment.ScheduledArrivalTime)) || !IsValidDate(Convert.ToString(segment.ScheduledArrivalTime)))
                    {
                        errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.SchArrTimeFormat), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                        response = new Response<string>()
                        {
                            Status = Status.BusinessException,
                            Errors = errorList
                        };
                    }

                }
            }
            return response;

        }
        public static bool IsValidDate(string date)
        {
            if (DateTime.TryParse(date.Trim(), out DateTime datetime))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsValidEmailId(string email)
        {

            try
            {
                System.Net.Mail.MailAddress emailaddr = new(email.Trim());
                return emailaddr.Address.Equals(email);
            }
            catch
            {
                return false;
            }
        }
    }
}