using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;

namespace VAA.EAPI.NDC.Offer.Validations.OrderQuote
{
    public class OrderQuoteRequestValidations
    {
        public static Response<IATA_OrderReshopRS> ValidateAgency(ValidationRS validationRS, IHostEnvironment hostingEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(RequestValidations.ValidateAgencyDetails(validationRS));

            if (errorIds.Count == 0)
            {
                errorIds.AddRange(RequestValidations.ValidateAgentTicketingAuthority(validationRS));
            }

            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(errorIds, hostingEnvironment);
        }

        public static Response<IATA_OrderReshopRS> ValidateUpsellConfigResponse(IEnumerable<UpsellOffersData> upsellConfigResponse, IHostEnvironment hostingEnvironment)
        {
            var errorIds = ValidateOfferPrice.ValidateUpsellConfigResponse(upsellConfigResponse);

            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(errorIds, hostingEnvironment);
        }

        public static Response<IATA_OrderReshopRS> ValidateOrderQuoteRequest(IATA_OrderQuoteRQ orderQuoteRQ, IHostEnvironment hostEnvironment)
        {
            var errorIds = MandatoryFieldsValidations.ValidateOrderQuoteRequest(orderQuoteRQ.DistributionChain);

            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<IATA_OrderReshopRS> ValidateMandatoryFields(OrderRepriceQuery request, IHostEnvironment hostingEnvironment)
        {

            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(MandatoryFieldsValidations.ValidateRequestFields(request));

            if (errorIds.Count == 0)
            {
                errorIds.AddRange(MandatoryFieldsValidations.ValidateOrderIDinRQ(request));
                errorIds.AddRange(MandatoryFieldsValidations.ValidateOfferIDinRQ(request));
            }

            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(errorIds, hostingEnvironment);
        }

        public static Response<IATA_OrderReshopRS> ValidateMandatoryRequestFields(OrderRepriceQuery request, IHostEnvironment hostingEnvironment)
        {

            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(MandatoryFieldsValidations.ValidateOrdinalinRequestFields(request));

            if (errorIds.Count == 0)
            {
                errorIds.AddRange(MandatoryFieldsValidations.ValidateOrderIDinRQ(request));
            }

            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(errorIds, hostingEnvironment);
        }

        public static Response<IATA_OrderReshopRS> ValidateOrderReshopDB(OrderRepriceQuery orderRepriceRQ, OfferResponse offerDoc, IHostEnvironment hostingEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(MandatoryFieldsValidations.ValidateOfferItemIDinDB(orderRepriceRQ, offerDoc));

            if (errorIds.Count == 0)
            {
                errorIds.AddRange(MandatoryFieldsValidations.ValidateOrderDB(orderRepriceRQ, offerDoc));
                errorIds.AddRange(MandatoryFieldsValidations.ValidatePaxList(orderRepriceRQ, offerDoc));
                errorIds.AddRange(MandatoryFieldsValidations.ValidateOrderDBServiceBeneficiaries(orderRepriceRQ, offerDoc));
            }

            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(errorIds, hostingEnvironment);
        }

        public static Response<IATA_OrderReshopRS> ValidateOrderQuoteHoldBookingRequest(OrderDocument orderDocument, IHostEnvironment hostingEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(MandatoryFieldsValidations.ValidateOrderDetails(orderDocument));

            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(errorIds, hostingEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateOrderQuoteOrderWarnings(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(ValidateOrderWarnings(orderDocument), hostEnvironment);
        }

        private static IEnumerable<NDCWarning> ValidateOrderWarnings(OrderDocument orderDocument)
        {
            return orderDocument.Warnings.Select(warning => new NDCWarning
            {
                Code = warning.Code,
                ShortText = warning.Message,
                Status = warning.Status,
                TagText = warning.Type
            });
        }

        public static Response<IATA_OrderReshopRS> ValidateHoldBookingTTL(OrderDocument orderDocument, IHostEnvironment hostingEnvironment)
        {
            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(ValidateTTL(orderDocument), hostingEnvironment);
        }

        private static List<ErrorDetail> ValidateTTL(OrderDocument orderDocument)
        {
            var errorIds = new List<ErrorDetail>();

            if (Convert.ToDateTime(orderDocument?.TimeLimits?.FirstOrDefault()?.Limit) < DateTime.Now)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "303_1",
                });
            }

            return errorIds;
        }

        public static Response<IATA_OrderReshopRS> ValidateOrderCancelledStatus(OrderDocument orderDocument, IHostEnvironment hostingEnvironment)
        {
            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(ValidateCancelledStatus(orderDocument), hostingEnvironment);
        }

        private static List<ErrorDetail> ValidateCancelledStatus(OrderDocument orderDocument)
        {
            var errorIds = new List<ErrorDetail>();

            if (orderDocument?.Warnings?.Any(x => x.Code == Constants.Application.OrderCancelledErrorCode && x.Message == Constants.Application.OrderCancelledTagText) == true)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "303_2",
                });
            }

            return errorIds;
        }

        public static Response<T> ValidateSellerFollowUpAction<T>(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();
            if (!(orderDocument?.OrderItems?.Any(oi => oi?.SellerFollowUpAction?.Any(a => a.Is("Reshop")) == true) == true) && orderDocument?.DataList?.SegmentDetails?.Any(x => x.ScheduleChangeIndicator) == true)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "915_2",
                });
            }
            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }
    }
}