using Gremlin.Net.Process.Traversal;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;

namespace VAA.EAPI.NDC.Offer.Validations.OrderReshopNameCorrection
{
    public static class OrderReshopNameCorrectionValidation
    {
        public static Response<T> ValidateNameCorrectionRequest<T>(IATA_OrderReshopRQ orderReshopRequest, OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(ValidateOrderReshop.ValidateNameCorrectionRequest(orderReshopRequest, orderDocument));
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

        public static IEnumerable<NDCWarning> ValidateOrderReshopNameCorrectionOrderWarnings(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(ValidateOrderReshop.ValidateReshopOrderWarnings(orderDocument), hostEnvironment);
        }

        public static Response<T> ValidateNameCorrectionRules<T>(OrderDocument orderDocument, OrderReshopRequest orderReshopRequest, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(ValidateOrderReshop.ValidateNamingConvention(orderReshopRequest.PaxNameChangeDetails));
            
            if (errorIds.IsNullOrEmpty()) errorIds.AddRange(ValidateOrderReshop.ValidateNameChanged(orderDocument, orderReshopRequest.PaxNameChangeDetails.PaxRefID));
            if (errorIds.IsNullOrEmpty()) errorIds.AddRange(ValidateOrderReshop.ValidateFlightSegmentStatus(orderDocument));
            if (errorIds.IsNullOrEmpty()) errorIds.AddRange(ValidateOrderReshop.ValidateEligibleSegment(orderDocument.DataList.SegmentDetails));
            if (errorIds.IsNullOrEmpty()) errorIds.AddRange(ValidateOrderReshop.ValidateFullSurnameChange(orderDocument, orderReshopRequest.PaxNameChangeDetails));
            
            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

    }
}