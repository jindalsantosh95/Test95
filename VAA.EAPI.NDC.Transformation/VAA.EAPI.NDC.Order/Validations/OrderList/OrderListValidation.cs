using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;

namespace VAA.EAPI.NDC.Order.Validations.OrderList
{
    public static class OrderListValidation
    {
        public static Response<T> ValidateOrderListRequest<T>(IATA_OrderListRQ request, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderListRequestValidations.ValidateFilterCombinations<T>(request));

            if(errorIds.IsNullOrEmpty()) errorIds.AddRange(OrderListRequestValidations.ValidateMandatoryFields<T>(request));

            if(errorIds.IsNullOrEmpty()) errorIds.AddRange(OrderListRequestValidations.ValidateDateRange<T>(request));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }
    }
}
