using Gremlin.Net.Process.Traversal;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Order.Validations.Common;

namespace VAA.EAPI.NDC.Order.Validations.OrderCreate
{
    public class OrderCreateValidation
    {

        public static Response<T> OrderCreateRQValidation<T>(IATA_OrderCreateRQ orderCreateRQ, bool holdBooking, bool motoPayment, bool cardValidation, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            if (errors.IsNullOrEmpty()) errors.AddRange(OrderCreateRequestValidation.ValidateDistributionChain(orderCreateRQ));

            if (errors.IsNullOrEmpty()) errors.AddRange(OrderCreateRequestValidation.ValidateOfferID(orderCreateRQ));

            if (!holdBooking) errors.AddRange(LuhnCheckValidation.ValidateCard(orderCreateRQ.Request?.PaymentFunctions));

            if (!holdBooking) errors.AddRange(OrderCreateRequestValidation.ValidateContactDetails(orderCreateRQ));

            if (!holdBooking && cardValidation) errors.AddRange(OrderCreateRequestValidation.ValidateSecurePaymentVersion2(orderCreateRQ.Request?.PaymentFunctions));

            if(!holdBooking && !motoPayment && cardValidation) errors.AddRange(OrderCreateRequestValidation.ValidateSecurePaymentVersion2Data(orderCreateRQ.Request?.PaymentFunctions));

            if (errors.IsNullOrEmpty() && holdBooking) errors.AddRange(OrderCreateRequestValidation.ValidateNonFlightServices(orderCreateRQ));

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateOfferOrder<T>(IATA_OrderCreateRQ request, OfferResponse offerDoc, ValidationRS validationRS, bool holdBooking, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();
            bool iscashFOP = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is SettlementPlanType;
            bool isEasyPay = request.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is PaymentProcessingIATA_EasyPayType;
            
            errors.AddRange(OrderCreateRequestValidation.ValidateOffer(request, offerDoc));

            if (offerDoc != null)
            {
                if(!iscashFOP && !isEasyPay) errors.AddRange(OrderCreateRequestValidation.ValidateCreditCardFOP(request, offerDoc.Parties.Participants.First().IsCreditCardAccepted));
                errors.AddRange(OrderCreateRequestValidation.ValidateAgentForHoldBooking(request, offerDoc, validationRS));
                if (!holdBooking) errors.AddRange(OrderCreateRequestValidation.ValidateAgent(request, validationRS));
                errors.AddRange(OrderCreateRequestValidation.ValidateDuplicatePaxID(request));

                if (errors.Count == 0)
                {
                    errors.AddRange(OrderCreateRequestValidation.ValidatePax(request, holdBooking, isEasyPay));
                    if (holdBooking) errors.AddRange(OrderCreateRequestValidation.ValidateAmountForHoldBooking(request, offerDoc));
                    if (!holdBooking)
                    {
                        errors.AddRange(OrderCreateRequestValidation.ValidateAmount(request, offerDoc));
                        errors.AddRange(OrderCreateRequestValidation.ValidateAgent(request, validationRS));
                        if (!iscashFOP && !isEasyPay) errors.AddRange(OrderCreateRequestValidation.ValidatePayerAddress(request));
                    }
                }
            }

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateGSTDetails(IATA_OrderCreateRQ request, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();

            warnings.AddRange(OrderCreateRequestValidation.ValidateGSTNumberandOragName(request));
            warnings.AddRange(OrderCreateRequestValidation.ValidateGSTPhoneEmailAddress(request));

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static bool ValidateGST(ValidationRS validationRS, OfferResponse offerResponse, IEnumerable<AirportDetail> airportDetails)
        {
            return OrderCreateRequestValidation.ValidateGST(validationRS, offerResponse, airportDetails);
        }

        public static IEnumerable<NDCWarning> ValidateAPISDetails(IATA_OrderCreateRQ request, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(OrderCreateRequestValidation.ValidateAPISDetails(request), hostEnvironment);
        }

        public static Response<T> ValidateContactDetails<T>(IATA_OrderCreateRQ request, OfferResponse offerDoc, bool holdBooking, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(OrderCreateRequestValidation.ValidateContactInfoName(request));
            errorIds.AddRange(OrderCreateRequestValidation.ValidateEmailAndPhone<T>(request));
            if (!holdBooking) errorIds.AddRange(OrderCreateRequestValidation.ValidateBirthDate<T>(request, offerDoc));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateFlightServiceList<T>(IATA_OrderCreateRQ orderCreateRequest, FlightOfferServiceRS flightOfferServiceRS, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(ValidateFlightServieListResponse.ValidateFlightServiceListErrorResponse(flightOfferServiceRS));

            if (!errorIds.Any())
                errorIds.AddRange(ValidateFlightServieListResponse.ValidateFlightServiceListResponseOrderCreate(flightOfferServiceRS, orderCreateRequest.DistributionChain.OrderBy(x => x.Ordinal).ToList(), orderCreateRequest.Request.CreateOrder.Item as Domain.OrderCreateRQ.AcceptSelectedQuotedOfferListType));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateSeatOfferDetails<T>(IATA_OrderCreateRQ orderCreateRequest, IEnumerable<FlightSegmentSeatMap> seat, IHostEnvironment hostEnvironment)
        {
            var errorIds = OrderCreateRequestValidation.ValidateSeatOffer(orderCreateRequest, seat);

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateSeatCCFOP(IATA_OrderCreateRQ orderCreateRequest, string seatOfferId ,IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();
            bool addWarning = false;

            var seatOffer = (orderCreateRequest.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType)?
                .SelectedPricedOffer?
                .FirstOrDefault(x => x.OfferRefID == seatOfferId && x.SelectedOfferItem.Any(y => y.SelectedSeat != null));

            var isFreeSeat = seatOffer != null && seatOffer.TotalOfferPriceAmount?.Value == 0;

            if (orderCreateRequest.Request.PaymentFunctions.Length > 1)
            {
                if(orderCreateRequest.Request.PaymentFunctions.Any(p => p.OfferAssociation == null || p.OfferAssociation?.FirstOrDefault()?.OfferRefID == null))
                    warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "911_25", Type = "Warning" });

                var seatPaymentFunction = orderCreateRequest.Request.PaymentFunctions.FirstOrDefault(x => x.OfferAssociation?.FirstOrDefault()?.OfferRefID == seatOfferId);
                if(seatPaymentFunction == null || seatPaymentFunction?.PaymentMethodCriteria?.PaymentTypeCode != "CC")
                    addWarning = true;
            }
            else addWarning = true;

            if (addWarning && !isFreeSeat)
            {
                warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "466_4", Type = "Warning" });
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateSSROrderItems(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();

            if (!orderDocument.Warnings.IsNullOrEmpty())
            {
                if (orderDocument.Warnings.Any(x => x.Code == "185")) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "185_1", Type = "Warning" });

                if (orderDocument.Warnings.Any(x => x.Code == "182")) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "182_1", Type = "Warning" });

                if (orderDocument.Warnings.Any(x => x.Code == "186")) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "185_2", Type = "Warning" });
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static Response<T> ValidateIFGAgentsPermission(IATA_OrderCreateRQ request, IFGAgentFopLookUpResponse ifgAgentFopLookUpResponse, IHostEnvironment hostEnvironment)
        {
            var errorIds = OrderCreateRequestValidation.ValidateIFGAgentsPermissionErrorResponse(request, ifgAgentFopLookUpResponse);

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> PaymentReportingFailureWarning(IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>
            {
                new() { IsDynamicMessage = false, Code = "76H_10", Type = "Warning" }
            };

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ClidInactiveWarning(IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>
            {
                new() { IsDynamicMessage = false, Code = "76H_11", Type = "Warning" }
            };

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }
    }
}
