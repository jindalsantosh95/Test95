using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Validations.OrderListMS
{
    public static class OrderListMSResponseValidations
    {
        public static Response<T> ValidateOrderDataResponse<T>(IEnumerable<OrderListResponse> orderListResponse, IHostEnvironment hostEnvironment)
        {
            var errors = new[]
            { 
                new ErrorDetail 
                { 
                    IsDynamicMessage = false, ErrorCode = "129_5" 
                } 
            };

            return ErrorHelper<T>.GetErrors(errors , hostEnvironment);
        }
    }
}
