using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Constants;

namespace VAA.EAPI.Microservices.Retail.Offer.Validation.Ita
{
    public class GetITAErrorsFromResponse
    {
        public static Result<Response<string>> GetITAErrors(result result, bool isReprice = false)
        {
            var errorList = new List<ErrorDetails>();
            var response = new Response<string>
            {
                Status = Status.OK,
                Errors = null
            };

            if (result.messages?.error != null)
            {
                foreach (var x in result.messages.error)
                {
                    response = GetErrorResponse(errorList, isReprice ? ErrorCodes.RepriceBusinessCode : ErrorCodes.ReshopBusinessCode, $"{ErrorCodes.ErrorCode} : {x.code} {ErrorCodes.ErrorMessage} : {x.Value}");
                }
            }

            if (result.messages?.warning != null)
            {
                foreach (var x in result.messages.warning)
                {
                    response = GetErrorResponse(errorList, isReprice ? ErrorCodes.RepriceBusinessCode : ErrorCodes.ReshopBusinessCode, $"{ErrorCodes.ErrorCode} : {x.code} {ErrorCodes.ErrorMessage} : {x.Value}");
                }
            }

            if (result.error != null)
            {
                response = GetErrorResponse(errorList, isReprice ? ErrorCodes.RepriceBusinessCode : ErrorCodes.ReshopBusinessCode, result.error.message);
            }

            return response;
        }
        private static Response<string> GetErrorResponse(List<ErrorDetails> errorList, string errorCode, string message)
        {
            errorList.Add(ErrorMapper.MapITAExceptionToEAPIException(errorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, message).FirstOrDefault());
            return new Response<string>
            {
                Status = Status.BusinessException,
                Errors = errorList
            };
        }
    }
}
