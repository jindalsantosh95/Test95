using Gremlin.Net.Process.Traversal;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;

namespace VAA.EAPI.NDC.Offer.Validations.OrderReshopCancel
{
    public static class OrderReshopCancelRequestValidations
    {
        public static Response<T> ValidateOrderReshopCancelOrderRefIDRequest<T>(IATA_OrderReshopRQ orderReshopRequest, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(ValidateOrderReshop.ValidateRequestOrderId( orderReshopRequest));
            if (errorIds.Count == 0)
            {
                errorIds.AddRange(ValidateOrderReshop.ValidateRequestOrderRefIds(orderReshopRequest));
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOrderReshopCancelVaRequest<T>(OrderReshopRequest orderReshopRQ,IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(ValidateOrderReshop.ValidateOrderReshopRQDistributionEntity(orderReshopRQ));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateAgency<T>(ValidationRS validationRS, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(RequestValidations.ValidateAgencyDetails(validationRS), hostEnvironment);
        }

        public static Response<T> ValidateOrderDocument<T>(OrderReshopRequest orderRequest, OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(ValidateOrderReshop.ValidateReshopOrderID(orderRequest, orderDocument), hostEnvironment);
        }

        public static Response<T> ValidateHoldBooking<T>(bool isHoldBooking, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            if (!isHoldBooking)
            {
                return null;
            }
            else
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "915_3"
                });
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateOrderReshopCancelOrderWarnings(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(ValidateOrderReshop.ValidateReshopOrderWarnings(orderDocument), hostEnvironment);
        }

        public static Response<T> ValidateAgentForRFC<T>(OrderDocument orderDocument, OrderReshopRequest orderReshopRQ, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(ValidateOrderReshop.ValidateRFCAgent(orderReshopRQ, orderDocument), hostEnvironment);
        }

        public static Response<T> ValidateSegmentDetails<T>(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(ValidateOrderReshop.ValidateSegmantStatus(orderDocument), hostEnvironment);
        }

        public static Response<T> ValidateDisruptedPnr<T>(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(ValidateOrderReshop.ValidateSellerFollowUpAction(orderDocument), hostEnvironment);
        }
    }
}
