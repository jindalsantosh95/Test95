using Gremlin.Net.Process.Traversal;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Validations.Common;

namespace VAA.EAPI.NDC.Order.Validations.OrderRetrieve
{
    public static class OrderRetrieveValidation
    {
        public static Response<T> OrderRetrieveRQValidation<T>(IATA_OrderRetrieveRQ orderRetrieveRQ, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            errors.AddRange(OrderRetrieveRequestValidation.ValidateDistributionChain(orderRetrieveRQ));

            if (errors.IsNullOrEmpty()) errors.AddRange(OrderRetrieveRequestValidation.ValidateRLOCAndOrderID(orderRetrieveRQ));

            if (errors.IsNullOrEmpty()) errors.AddRange(OrderRetrieveRequestValidation.ValidateOrderIDFormat(orderRetrieveRQ));

            if (errors.IsNullOrEmpty()) errors.AddRange(OrderRetrieveRequestValidation.ValidateRLOCFormat(orderRetrieveRQ));

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> OrderRetrieveRQAgentValidation<T>(IATA_OrderRetrieveRQ orderRetrieveRQ, Infrastructure.Model.ValidationMS.ValidationRS validationMSResponse, IHostEnvironment hostEnvironment)
        {
            var errors = OrderRetrieveRequestValidation.ValidateAgent(orderRetrieveRQ, validationMSResponse);

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> OrderSearchRSValidation<T>(Air4OrderSearchMSResponse orderSearchResponse, string iataNum , bool skipGDSAgentValidation ,IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            errors.AddRange(OrderSearchResponseValidation.ValidateOrderSearchNullResponse(orderSearchResponse));

            if (!errors.Any() && orderSearchResponse.ErrorDetails != null) errors.AddRange(OrderSearchResponseValidation.ValidateOrderSearchRS(orderSearchResponse.ErrorDetails));

            if (!errors.Any()) errors.AddRange(OrderSearchResponseValidation.ValidateOrderSearchRSForVSTicks(orderSearchResponse.OrderSearchResponse));

            if (!errors.Any() && skipGDSAgentValidation) errors.AddRange(OrderSearchResponseValidation.ValidateOrderSearchRSForGDSAgent(orderSearchResponse.OrderSearchResponse, iataNum));

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateDisruptedServices(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            if(orderDocument is not null && orderDocument.DataList.SegmentDetails.Any(s => s.Status?.Code == Constants.Application.FlightStatusCodeCancelled) && orderDocument.OrderHistory != null)
            {
                var latestOrderHistory = orderDocument.OrderHistory.OrderBy(o => o.Version).Last();
                var disruptedSegs = orderDocument.DataList.SegmentDetails.Where(s => s.Status.Code == Constants.Application.FlightStatusCodeCancelled).Select(x => x.SegmentId);

                if(latestOrderHistory.OrderItems.Any(oi => oi.Services.First().Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && disruptedSegs.Contains(oi.Services.First().SegmentRefIds.First())) || latestOrderHistory.OrderItems.Any(oi => oi.Services.First().OrderInstructionRefs != null && oi.Services.SelectMany(x => x.SegmentRefIds).Intersect(disruptedSegs) != null))
                {
                    return ErrorHelper<NDCWarning>.GetWarnings([new NDCWarning() { Code = "214_1"}], hostEnvironment);
                }
            }

            return [];
        }

        public static Response<T> OrderSearchRSValidation(OrderDocument getReservationOrderDoc, IHostEnvironment hostEnvironment)
        {
            var errors = OrderRetrieveRequestValidation.OrderSearchRSValidation(getReservationOrderDoc);

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> SSRWarnings(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();

            if (!orderDocument.Warnings.IsNullOrEmpty())
            {
                if (orderDocument.Warnings.Any(x => x.Code == "182")) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "182_1", Type = "Warning" });

                if (orderDocument.Warnings.Any(x => x.Code == "186")) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "185_2", Type = "Warning" });
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }
    }
}
