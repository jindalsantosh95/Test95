
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Validations.Common;

namespace VAA.EAPI.NDC.Order.Validations.OrderHistory
{
    public static class OrderHistoryValidation
    {
        public static Response<T> OrderHistoryRQValidation<T>(IATA_OrderHistoryRQ OrderHistoryRQ, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            errors.AddRange(OrderHistoryRequestValidation.ValidateDistributionChain(OrderHistoryRQ));

            if (errors.IsNullOrEmpty()) errors.AddRange(OrderHistoryRequestValidation.ValidateRLOCAndOrderID(OrderHistoryRQ));

            if (errors.IsNullOrEmpty()) errors.AddRange(OrderHistoryRequestValidation.ValidateOrderIDFormat(OrderHistoryRQ));

            if (errors.IsNullOrEmpty()) errors.AddRange(OrderHistoryRequestValidation.ValidateRLOCFormat(OrderHistoryRQ));

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> OrderHistoryRQAgentValidation<T>(IATA_OrderHistoryRQ OrderHistoryRQ, Infrastructure.Model.ValidationMS.ValidationRS validationMSResponse, IHostEnvironment hostEnvironment)
        {
            var errors = OrderHistoryRequestValidation.ValidateAgent(OrderHistoryRQ, validationMSResponse);

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> OrderSearchRSValidation<T>(Air4OrderSearchMSResponse orderSearchResponse, string iataNum, bool skipGDSAgentValidation, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            errors.AddRange(OrderSearchResponseValidation.ValidateOrderSearchNullResponse(orderSearchResponse));

            if (!errors.Any() && orderSearchResponse.ErrorDetails != null) errors.AddRange(OrderSearchResponseValidation.ValidateOrderSearchRS(orderSearchResponse.ErrorDetails));

            if (!errors.Any()) errors.AddRange(OrderSearchResponseValidation.ValidateOrderSearchRSForVSTicks(orderSearchResponse.OrderSearchResponse));

            if (!errors.Any() && skipGDSAgentValidation) errors.AddRange(OrderSearchResponseValidation.ValidateOrderSearchRSForGDSAgent(orderSearchResponse.OrderSearchResponse, iataNum));

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidatePaymentFailure(OrderDocument historyOrderDocument, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();

            foreach(var orderHistory in historyOrderDocument.OrderHistory)
            {
                if ((orderHistory.Warnings?.Any(x => x.Code == "911") ?? false) && !(warnings?.Any(x => x.Code == "911_49") ?? false)) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "911_49", Type = "Warning" });
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateDisruptedServices(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            if (orderDocument.DataList.SegmentDetails.Any(s => s.Status?.Code == Constants.Application.FlightStatusCodeCancelled) && orderDocument.OrderHistory != null)
            {
                var latestOrderHistory = orderDocument.OrderHistory.OrderBy(o => o.Version).Last();
                var disruptedSegs = orderDocument.DataList.SegmentDetails.Where(s => s.Status.Code == Constants.Application.FlightStatusCodeCancelled).Select(x => x.SegmentId);

                if (latestOrderHistory.OrderItems.Any(oi => oi.Services.First().Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum && disruptedSegs.Contains(oi.Services.First().SegmentRefIds.First())) || latestOrderHistory.OrderItems.Any(oi => oi.Services.First().OrderInstructionRefs != null && oi.Services.SelectMany(x => x.SegmentRefIds).Intersect(disruptedSegs) != null))
                {
                    return ErrorHelper<NDCWarning>.GetWarnings([new NDCWarning() { Code = "214_1" }], hostEnvironment);
                }
            }

            return [];
        }

        public static IEnumerable<NDCWarning> ValidateSSROrderItems(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();
            
            if (orderDocument.OrderHistory.Any(x => x.Warnings != null && x.Warnings.Any(w => w.Code == "182")))
            {
                warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "182_1", Type = "Warning" });
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }
    }
}
