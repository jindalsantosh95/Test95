using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Clid;

namespace VAA.EAPI.NDC.Order.Infrastructure.Validations.ConfigMS
{
    public static class ConfigMSResponseValidation
    {
        public static Response<T> ValidateResponses<T>(CorporateData configMSRS, IHostEnvironment hostEnvironment)
        {
            if (!configMSRS.CbaVsActiveContract)
            {
                var errors = new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_11"
                    }
                };

                return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
            }

            return null;
        }
    }
}