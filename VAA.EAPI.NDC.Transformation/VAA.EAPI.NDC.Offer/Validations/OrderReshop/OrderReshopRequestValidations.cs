using Gremlin.Net.Process.Traversal;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.ReshopFareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Offer.Validations.OrderReshop
{
    public static class OrderReshopRequestValidations
    {
        public static Response<T> ValidateOrderReshopVaRequest<T>(OrderReshopRequest orderReshopRQ, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(ValidateOrderReshop.ValidateOrderReshopRQDistributionEntity(orderReshopRQ));
            errorIds.AddRange(ValidateOrderReshop.ValidateOrderReshopRQOrderID(orderReshopRQ?.OrderId));
            errorIds.AddRange(ValidateOrderReshop.ValidateOriginDestCriteria(orderReshopRQ));
            errorIds.AddRange(ValidateOrderReshop.ValidateOrderReshopRQDepartureDate(orderReshopRQ));
            errorIds.AddRange(ValidateOrderReshop.ValidateOrderReshopRQCabinPreferance(orderReshopRQ));
            errorIds.AddRange(ValidateOrderReshop.ValidateOrderReshopRQOrderItemID(orderReshopRQ));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateAgency<T>(ValidationRS validationRS, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(RequestValidations.ValidateAgencyDetails(validationRS), hostEnvironment);
        }


        public static Response<T> ValidateOrderReshopRequestAgainstDocument<T>(bool isHoldBookingReshop, OrderReshopRequest orderRequest, OrderDocument orderDocument, IEnumerable<ReshopFareFamily> reshopFareFamilies, AgencyValidateResponse validateResponse, IHostEnvironment hostEnvironment, bool isReshopDisruption, int ticketValidityWindowInDays)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(ValidateOrderReshop.ValidateReshopOrderID(orderRequest, orderDocument));
            //errorIds.AddRange(ValidateOrderReshop.ValidateAgentIDReshop(validateResponse, orderDocument));
            errorIds.AddRange(ValidateOrderReshop.ValidateReshopPNR(orderRequest, orderDocument));
            errorIds.AddRange(ValidateOrderReshop.ValidateReshopOnd(orderRequest, orderDocument));
            errorIds.AddRange(ValidateOrderReshop.ValidateReshopDateChronology(orderRequest, orderDocument, isReshopDisruption)); 
            errorIds.AddRange(ValidateOrderReshop.ValidateReshopCabinCode(orderRequest, orderDocument, reshopFareFamilies));
            errorIds.AddRange(ValidateOrderReshop.ValidateReshopOrderItemCount(orderRequest, orderDocument));
            errorIds.AddRange(ValidateOrderReshop.ValidateReshopOrderItemID(orderRequest, orderDocument));
            errorIds.AddRange(ValidateOrderReshop.ValidateReshopServiceID(orderRequest, orderDocument));

            if (isHoldBookingReshop && !isReshopDisruption && errorIds.Count == 0)
            {
                errorIds.AddRange(
                    ValidateOrderReshop.ValidateReshopHoldBooking(orderDocument));
            }
            else if (!isHoldBookingReshop)
            {
                errorIds.AddRange(
                    ValidateOrderReshop.ValidateSegmentStatus(orderRequest, orderDocument));

                errorIds.AddRange(
                    ValidateOrderReshop.ValidateFlownSegment(orderRequest, orderDocument));
            }

            if (isReshopDisruption && errorIds.Count == 0)
            {
                errorIds.AddRange(ValidateOrderReshop.ValidateDisruptionScopeAndTicketIssueDate(orderRequest, orderDocument, ticketValidityWindowInDays));
            }

            if (errorIds.Count == 0)
            {
                errorIds.AddRange(ValidateOrderReshop.ValidateRLOCFormat(orderRequest));
                errorIds.AddRange(ValidateOrderReshop.ValidateIfRLOCOrdeIDMatch(orderRequest, orderDocument));
                errorIds.AddRange(ValidateOrderReshop.ValidateOrderIDFormat(orderRequest));
                errorIds.AddRange(ValidateOrderReshop.ValidateOrderStatus(orderDocument));
            }
            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOrderRFCReshopRequestAgainstDocument<T>(OrderReshopRequest orderRequest, OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();
            
            errorIds.AddRange(ValidateOrderReshop.ValidateSegmentStatus(orderRequest, orderDocument));
            
            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOrderDocument<T>(OrderReshopRequest orderRequest, OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(ValidateOrderReshop.ValidateReshopOrderID(orderRequest, orderDocument), hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateOrderReshopOrderWarnings(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(ValidateOrderReshop.ValidateReshopOrderWarnings(orderDocument), hostEnvironment);
        }
        public static IEnumerable<NDCWarning> ValidateOrderReshopSeatDeleteWarnings(OrderDocument sabreOrderDoc, OrderReshopRequest orderReshopRQ, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(ValidateOrderReshop.ValidateReshopSeatDeleteWarnings(sabreOrderDoc, orderReshopRQ), hostEnvironment);
        }
        
        public static IEnumerable<NDCWarning> ValidateOrderReshopAPISRemovalWarnings(OrderDocument sabreOrderDoc, OrderReshopRequest orderReshopRQ, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(ValidateOrderReshop.ValidateReshopAPISWarnings(sabreOrderDoc, orderReshopRQ), hostEnvironment);
        }

        public static Response<T> ValidateSabreOrderDocSegmentCouponStatus<T>(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(ValidateOrderReshop.ValidateSegmentCouponStatus(orderDocument), hostEnvironment);
        }

        public static Response<T> ValidateOrderReshopRequest(IATA_OrderReshopRQ orderReshopRQ, IHostEnvironment hostEnvironment)
        {
            var errorIds = ValidateOrderReshop.ValidateOrderReshopRequest(orderReshopRQ.DistributionChain);

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateAgentInRequest(OrderDocument orderDocument, DistributionChainLinkType[] distributionChain, IHostEnvironment hostEnvironment)
        {
            var errorIds = ValidateOrderReshop.ValidateAgencyInRequest(orderDocument, distributionChain);

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
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
