using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Validations.OrderMS
{
    public static class OrderMSResponseValidation
    {
        public static Response<T> ValidateResponses<T>(OrderRS orderRS, IHostEnvironment hostEnvironment)
        {
            var errors = orderRS.Errors.Select(error =>
            {
                if (error?.Code == "F001")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_4"
                    };
                }
                else if (error?.Code == "F002")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_53"
                    };
                }
                else
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_22"
                    };
                }
            });

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }
    }
}
