using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class OtpTokenValidationService : IOtpTokenValidationService
    {
        async Task<Response<string>> IOtpTokenValidationService.ValidaateOtpToken(OtpTokenValidationRequest request)
        {
            if (request.Token.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.Token));

            try
            {
                var tokenResponse = await ValidationTokenHelper.GetEmailAddress(request?.Token);

                if (tokenResponse.IsNullOrEmpty())
                    return GetErrorResponse(Constants.ErrorCodes.EnrolmentTokenValidationFail);

                return new Response<string>()
                {
                    Status = Status.OK,
                    Value = tokenResponse,
                    Errors = null
                };
            }

            catch
            { 
                return GetErrorResponse(Constants.ErrorCodes.EnrolmentTokenValidationFail);
            }
        }

        private static Response<string> GetErrorResponse(string errorCode)
        {
            List<ErrorDetails> errorList =
            [
                ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault(),
            ];

            return new Response<string>()
            {
                Status = Status.BusinessException,
                Value = null,
                Errors = errorList
            };
        }
    }
}
