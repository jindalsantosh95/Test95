using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Constants;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using System.Text.RegularExpressions;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using Status = VAA.EAPI.Domain.Entities.Common.Status;
using System.Globalization;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Validations
{
    public static class OrderChangeRequestValidator
    {
        public static Result<Response<string>> ValidateRequest(OrderChangeRequestData orderChangeRequestData)
        {
            var errorList = new List<ErrorDetails>();
            var response = new Response<string>()
            {
                Status = Status.OK,
                Errors = null
            };

            if (orderChangeRequestData.ChangeType != null) return response;

            //Skip validation for Post sale seat flow.
            if (orderChangeRequestData.OrderAmendments.Any(x => x.ProductType.ToString() == Enum.GetName(OrderAmendment.ProductTypes.SeatEnum)))
                return response;

            //validating customerDetails
            if (orderChangeRequestData.DataList.CustomerDetails == null || !orderChangeRequestData.DataList.CustomerDetails.Any())
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
            if (orderChangeRequestData.OrderAmendments != null &&  !orderChangeRequestData.IsReshopDisruption && orderChangeRequestData.OrderAmendments.Any(x => x.CustomerReference == null || !x.CustomerReference.Any()))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MissingCustomerReferenceErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate customerId
            if (orderChangeRequestData.DataList.CustomerDetails != null && orderChangeRequestData.DataList.CustomerDetails.Any(x => string.IsNullOrEmpty(Convert.ToString(x.CustId))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.CustomerIDMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate FirstName
            if (orderChangeRequestData.DataList.CustomerDetails != null && orderChangeRequestData.DataList.CustomerDetails.Any(x => string.IsNullOrEmpty(x.FirstName)))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.FirstNameMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate LastName
            if (orderChangeRequestData.DataList.CustomerDetails != null && orderChangeRequestData.DataList.CustomerDetails.Any(x => string.IsNullOrEmpty(x.LastName)))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.LastNameMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate PtcLength
            if (orderChangeRequestData.DataList.CustomerDetails != null && (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.DataList.CustomerDetails.Any(x => string.IsNullOrEmpty(x.Ptc)) || orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.DataList.CustomerDetails.Any(x => x.Ptc.Length != Constants.Application.PtcLength)))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidPtcLength), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate OriginAirportCode
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.DataList.OriginDestinationAssociations.Any(x => string.IsNullOrEmpty(x?.OriginDestination.OriginAirport) || x?.OriginDestination.OriginAirport.Length != Constants.Application.IATAOriginAirportCodeLength) || orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.SelectMany(x => x.Segments).Any(y => string.IsNullOrEmpty(y.OriginAirportCode) || y.OriginAirportCode.Length != Constants.Application.IATAOriginAirportCodeLength)))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidOriginAirportCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            //validate DestinationAirportCode
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.DataList.OriginDestinationAssociations.Any(x => string.IsNullOrEmpty(x?.OriginDestination.DestinationAirport) || x?.OriginDestination.DestinationAirport.Length != Constants.Application.IATADestAirportCodeLength) || orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.SelectMany(x => x.Segments).Any(y => string.IsNullOrEmpty(y.DestinationAirportCode) || y.DestinationAirportCode.Length != Constants.Application.IATADestAirportCodeLength)))
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


            // validate MarketingCarrier length
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.SelectMany(x => x.Segments).Any(y => string.IsNullOrEmpty(y.MarketingCarrier) || y.MarketingCarrier.Length != Constants.Application.MarketingCarrierLength)))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMarketingCarrier), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            // validate MarketingFlightNumber
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.SelectMany(x => x.Segments).Any(y => string.IsNullOrEmpty(y.MarketingFlightNumber) || !(Constants.Application.MarketingFlightNumberMinLength <= y.MarketingFlightNumber.Length && y.MarketingFlightNumber.Length <= Constants.Application.MarketingFlightNumberMaxLength))))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMarketingFlightNumber), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            // validate ClassOfService
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null && x.FlightItems.SelectMany(x => x.Segments).Any(y => string.IsNullOrEmpty(y.ClassOfService) || y.ClassOfService.Length != Constants.Application.ClassOfServiceLength)))
            {
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.ClassOfServiceMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                response = new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = errorList
                };
            }

            // validate OfferRef Id
            if (orderChangeRequestData.OrderAmendments != null && !orderChangeRequestData.IsReshopDisruption && orderChangeRequestData.OrderAmendments.Any(x => x.OrderByReference == null || x.OrderByReference != null && x.OrderByReference.Any(y => string.IsNullOrEmpty(y.ItemsForAddition.OfferRefID))))
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
            if (orderChangeRequestData.DataList.CustomerDetails != null && orderChangeRequestData.DataList.CustomerDetails.Any())
            {
                Customer customer = orderChangeRequestData.DataList.CustomerDetails.First();
                var emailID = (customer.Contacts != null) ? customer.Contacts.Email: null;
                if(emailID != null)
                {
                    if (string.IsNullOrEmpty(emailID) || !IsValidEmailId(emailID.Trim()))
                    {
                        errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidEmailFromat), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                        response = new Response<string>()
                        {
                            Status = Status.BusinessException,
                            Errors = errorList
                        };
                    }
                }

            }

            // Validating proper Datetime format for relevant fields
            if (orderChangeRequestData.OrderAmendments != null && orderChangeRequestData.OrderAmendments.Any(x => x.FlightItems != null))
            {
                IEnumerable<Segment> segments = from orderamendment in orderChangeRequestData.OrderAmendments
                                                from flightitem in orderamendment.FlightItems
                                                from segment in flightitem.Segments
                                                select segment;
                foreach (Segment segment in segments)
                {
                    //ToDo : needs to implement the IsValidDate for format check
                    if (string.IsNullOrEmpty(segment.ScheduledDepartureTime?.LocalDateTime))
                    {
                        errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.SchDepTimeFormat), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                        response = new Response<string>()
                        {
                            Status = Status.BusinessException,
                            Errors = errorList
                        };
                    }
                    //ToDo : needs to implement the IsValidDate for format check

                    if (string.IsNullOrEmpty(segment.ScheduledArrivalTime?.LocalDateTime))
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
            return DateTime.TryParseExact(date, Constants.Application.InputDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
        public static bool IsValidEmailId(string email)
        {
            return RegexCompiled.IsMatch(email, Constants.Application.EmailRegex, RegexOptions.IgnoreCase);
        }
    }
}