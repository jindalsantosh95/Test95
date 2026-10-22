using Microsoft.Extensions.Hosting;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Validations.OrderManagementMS
{
    public static class OrderManagementMSResponseValidation
    {
        public static Response<T> ValidateResponses<T>(ChangeOrderRS changeOrderRS, IHostEnvironment hostEnvironment)
        {
            var errors = changeOrderRS.error.Select(error =>
            {
                if (error?.Code == "F001")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_4"
                    };
                }
                else if (error?.Code == "F003")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "114_1"
                    };
                }
                else if (error?.Code == "F004")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "185_3"
                    };
                }
                else if (error?.Code == "F005")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_52"
                    };
                }
                else if (error?.Code == "F006")
                {
                    return new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_54"
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

        public static Response<T> ChangeWarningsToError<T>(ChangeOrderRS changeOrderRS, IHostEnvironment hostEnvironment)
        {
            var warnings = changeOrderRS?.orderDocument?.Warnings?.Select(_ =>
            {
                return new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "185_2"
                };
            }) ?? Enumerable.Empty<ErrorDetail>();

            return ErrorHelper<T>.GetErrors(warnings, hostEnvironment);
        }
    }
}
