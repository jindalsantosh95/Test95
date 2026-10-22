using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using OriginDestinationAssociation = VAA.EAPI.Domain.Virgin.Models.Products.Flight.OriginDestinationAssociation;
using PriceDetail = VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop.PriceDetail;

namespace VAA.EAPI.Microservices.Retail.Offer.Validation.Ita
{
    public class OrderReshopRequestValidator
    {
        public static Result<Response<string>> ValidatorOrderReshopRequest(OrderReshopBundleQuery requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var response = new Response<string>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Errors = new()
            };
            //validate add filed
            response.Errors.AddRange(ValidateAddDetails(requestData.ReshopQueries?.FlightSpecific?.FlightsRequest?.Add, requestData.OriginDestinationAssociations));
            response.Errors.AddRange(ValidateRetainDetails(requestData.ReshopQueries?.FlightSpecific?.FlightsRequest?.Retain, requestData.OriginDestinationAssociations, requestData.SegmentDetails));
            response.Errors.AddRange(ValidateDeleteDetails(requestData.ReshopQueries?.FlightSpecific?.FlightsRequest?.Delete, requestData.OriginDestinationAssociations));

            var flightTypeOrder = requestData.ReshopOrder.Where(item => item.OrderProductType == Constants.Application.Flight).ToList();

            flightTypeOrder.ForEach(item =>
            {
                //validate PriceDetails
                response.Errors.AddRange(ValidatePriceDetails(item.priceDetails));
                //validate accountableDocumentRefs
                if (!requestData.IsReshopHoldBooking)
                {
                    response.Errors.AddRange(ValidateAccountableDocumentRefs(item.AccountableDocumentRefs));

                    // validate coupon details
                    item.AccountableDocumentRefs?.ForEach(docRef =>
                    {
                        response.Errors.AddRange(ValidateCouponDetails(docRef.Coupons, docRef.DocumentType));
                    });
                }
                //validate ServiceBeneficaries
                item.Services?.ForEach(service =>
                {
                    response.Errors.AddRange(ValidateServiceBeneficaries(service.ServiceBeneficiaries));
                });

            });

            //validate originDestinationAssociations
            response.Errors.AddRange(ValidateOriginDestinationAssociations(requestData.OriginDestinationAssociations));
            //validate customerDetails
            response.Errors.AddRange(ValidateCustomerDetails(requestData.CustomerDetails));
            //validate segmentDetails
            response.Errors.AddRange(ValidateSegmentDetails(requestData.SegmentDetails));

            if (response.Errors.Count > 0)
            {
                response.Status = EAPI.Domain.Entities.Common.Status.BusinessException;
            }

            return response;
        }

        private static List<ErrorDetails> ValidateServiceBeneficaries(List<string> serviceBeneficaries)
        {
            return serviceBeneficaries == null || serviceBeneficaries?.Count == 0 || serviceBeneficaries.FirstOrDefault().IsNullOrEmpty() ? GetErrorResponse(ErrorCodes.ServiceBeneficiaryIsMissing) : new();
        }

        private static List<ErrorDetails> ValidateAddDetails(List<Add> addDetails, List<OriginDestinationAssociation> originDestinationAssociations)
        {
            List<ErrorDetails> errorList = new();
            if (addDetails == null || addDetails?.Count == 0)
            {
                return GetErrorResponse(ErrorCodes.AddFieldMissing);
            }

            addDetails.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.OriginDestinationId))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.OriginDestinationIdIsMissing));
                }
                else if (originDestinationAssociations.All(a => a.OdLabel != item.OriginDestinationId))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.OnDIDInvalidAdd));
                }
            });

            return errorList;
        }

        private static List<ErrorDetails> ValidateRetainDetails(List<Retain> retainDetails, List<OriginDestinationAssociation> originDestinationAssociations, List<Segment> segmentDetails)
        {
            List<ErrorDetails> errorList = new();

            if (retainDetails == null)
            {
                return errorList;
            }

            retainDetails.ForEach(item =>
            {
                if (item.SegmentId.IsNullOrEmpty() || !item.SegmentId.Any() || item.SegmentId.Any(w => w.IsNullOrEmpty()) || segmentDetails.All(x => x.SegmentId != item.SegmentId.FirstOrDefault()))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.SegmentIDMissingorInvalid));
                }

                if (originDestinationAssociations.All(a => a.OdLabel != item.OriginDestinationId))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.OnDIDInvalidRetain));
                }
            });

            return errorList;
        }

        private static List<ErrorDetails> ValidateDeleteDetails(List<Delete> deleteDetails, List<OriginDestinationAssociation> originDestinationAssociations)
        {
            List<ErrorDetails> errorList = new();

            if (deleteDetails != null)
            {
                deleteDetails.ForEach(item =>
                {
                    if (originDestinationAssociations.All(a => a.OdLabel != item.OriginDestinationId))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.OnDIDInvalidDelete));
                    }
                });

            }
            return errorList;
        }

        private static List<ErrorDetails> ValidatePriceDetails(List<PriceDetail> priceDetails)
        {
            List<ErrorDetails> errorList = new();

            if (priceDetails?.Count == 0 || priceDetails == null)
            {
                return GetErrorResponse(ErrorCodes.PriceDetailsIsMissing);
            }

            priceDetails.ForEach(item =>
            {
                //validate priceComponentType 
                if (item.PriceComponentType == null || !Enum.IsDefined(typeof(PriceDetail.PriceComponentTypes), item.PriceComponentType))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.PriceComponentTypeIsMissing));
                }

                if (item.PriceComponentType is PriceDetail.PriceComponentTypes.Tax && string.IsNullOrEmpty(item.PriceComponentName))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.PriceComponentNameIsMissing));
                }

                //validate amount details
                errorList.AddRange(ValidateAmountDetails(item.Amount));
            });

            return errorList;
        }

        private static List<ErrorDetails> ValidateAmountDetails(CurrencyAmount amount)
        {
            List<ErrorDetails> errorList = new();

            if (amount == null)
            {
                return GetErrorResponse(ErrorCodes.AmountIsMissing);
            }

            if (string.IsNullOrEmpty(amount.MonetaryAmount))
            {
                errorList.AddRange(GetErrorResponse(ErrorCodes.MonetaryAmountIsMissing));
            }

            if (string.IsNullOrEmpty(amount.BaseCurrency))
            {
                errorList.AddRange(GetErrorResponse(ErrorCodes.BaseCurrencyIsMissing));
            }

            return errorList;
        }

        private static List<ErrorDetails> ValidateAccountableDocumentRefs(List<AccountableDocumentRef> accountableDocRef)
        {
            List<ErrorDetails> errorList = new();

            if (accountableDocRef == null || accountableDocRef?.Count == 0)
            {
                return GetErrorResponse(ErrorCodes.AccountableDocRefIsMissing);
            }

            if (accountableDocRef?.Count > 0)
            {
                accountableDocRef.ForEach(item =>
                {
                    if (string.IsNullOrEmpty(item.DocumentType))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.DocumentTypeIsMissing));
                    }

                    if (string.IsNullOrEmpty(item.DocumentNumber))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.DocumentNumberIsMissing));
                    }

                    if (item.DocumentType != null && !item.DocumentType.ToUpper().Equals(Constants.Application.EmdDocumentType)
                                                  && string.IsNullOrEmpty(item.FareCalculation))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.FareCalculationIsMissing));
                    }

                    if (string.IsNullOrEmpty(item.IssueDate))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.IssueDateIsMissing));
                    }
                });
            }

            return errorList;
        }

        private static List<ErrorDetails> ValidateCouponDetails(List<Coupon> couponDetails, string docType)
        {
            List<ErrorDetails> errorList = new();

            if (couponDetails == null || couponDetails.Count == 0)
            {
                return GetErrorResponse(ErrorCodes.CouponDetailsIsMissing);
            }

            if (couponDetails?.Count > 0)
            {
                couponDetails.ForEach(item =>
                {
                    if (docType != null && !docType.ToUpper().Equals(Constants.Application.EmdDocumentType) && string.IsNullOrEmpty(item.segmentRefId))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.SegmentRefIdIsMissing));
                    }

                    if (docType != null && !docType.ToUpper().Equals(Constants.Application.EmdDocumentType) && string.IsNullOrEmpty(item.CouponNumber))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.CouponNumberIsMissing));
                    }
                    if (string.IsNullOrEmpty(item.CouponStatus))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.CouponStatusMissing));
                    }

                    if (docType != null && !docType.ToUpper().Equals(Constants.Application.EmdDocumentType) && string.IsNullOrEmpty(item.FareBasis))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.FareBasisIsMissing));
                    }
                });
            }

            return errorList;
        }

        private static List<ErrorDetails> ValidateOriginDestinationAssociations(List<OriginDestinationAssociation> originDestinationAssociation)
        {
            List<ErrorDetails> errorList = new();

            if (originDestinationAssociation == null || originDestinationAssociation.Count == 0)
            {
                return GetErrorResponse(ErrorCodes.OriginDestinationAssociationIsMissing);
            }

            if (originDestinationAssociation.Count > 0)
            {
                originDestinationAssociation.ForEach(item =>
                {
                    if (string.IsNullOrEmpty(item.OdLabel))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.OdLabelIsMissing));
                    }

                    if (item.OriginDestination == null)
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.OriginDestinationIsMissing));
                    }

                    if (string.IsNullOrEmpty(item.OriginDestination?.OriginAirport))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.OriginAirportIsMissing));
                    }

                    if (string.IsNullOrEmpty(item.OriginDestination?.DestinationAirport))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.DestinationAirportIsMissing));
                    }

                    if (string.IsNullOrEmpty(item.OriginDestination?.DepartureDate))
                    {
                        errorList.AddRange(GetErrorResponse(ErrorCodes.OriginDestinationDateIsMissing));
                    }
                });

            }

            return errorList;
        }

        private static List<ErrorDetails> ValidateCustomerDetails(List<Customer> customerDetail)
        {
            List<ErrorDetails> errorList = new();

            if (customerDetail is not { Count: > 0 })
            {
                return errorList;
            }

            customerDetail.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.CustId))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.CustIdIsMissing));
                }

                if (string.IsNullOrEmpty(item.Ptc) || Constants.Application.Ptc.All(a => a != item.Ptc.ToUpper()))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.PtcIsMissing));
                }
            });

            return errorList;
        }

        private static List<ErrorDetails> ValidateSegmentDetails(List<Segment> segmentDetail)
        {
            List<ErrorDetails> errorList = new();

            if (segmentDetail is not { Count: > 0 })
            {
                return errorList;
            }

            segmentDetail.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.MarketingCarrier))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.MarketingCarrierIsMissing));
                }

                if (string.IsNullOrEmpty(item.MarketingFlightNumber))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.MarketingFlightNumberIsMissing));
                }

                if (string.IsNullOrEmpty(item.OriginAirportCode))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.OriginAirportCodeIsMissing));
                }

                if (string.IsNullOrEmpty(item.DestinationAirportCode))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.DestinationAirportCodeIsMissing));
                }

                if (string.IsNullOrEmpty(item.ClassOfService))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.ClassOfServiceIsMissing));
                }

                if (string.IsNullOrEmpty(item.SegmentNumber))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.SegmentNumberIsMissing));
                }

                if (string.IsNullOrEmpty(item.SegmentId))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.SegmentIDIsMissing));
                }

                if (item.ScheduledDepartureTime == null)
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.ScheduledDepartureTimeIsMissing));
                }

                if (string.IsNullOrEmpty(item.ScheduledDepartureTime?.LocalDateTime))
                {
                    errorList.AddRange(GetErrorResponse(ErrorCodes.LocalDateTimeIsMissing));
                }
            });

            return errorList;
        }

        private static List<ErrorDetails> GetErrorResponse(string errorCode)
        {
            return new()
            {
                ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
            };
        }

        public static Result<Response<string>> ValidatorOrderReshopNameChangeRequest(ReshopRequest requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var response = new Response<string>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Errors = new()
            };

            //New Validations to be added further
            if (response.Errors.Count > 0)
            {
                response.Status = EAPI.Domain.Entities.Common.Status.BusinessException;
            }

            return response;
        }

    }
}
