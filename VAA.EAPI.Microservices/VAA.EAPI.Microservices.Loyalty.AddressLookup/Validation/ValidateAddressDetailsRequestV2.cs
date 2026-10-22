using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Query;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Constants;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Validation
{
    public class ValidateAddressDetailsRequestV2
    {
        public static Result<Response<string>> AddressDetailsRequestValidator(AddressLookupQueryV2 request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var errorList = new List<ErrorDetails>();
            var response = new Response<string>

            {
                Status = Status.OK,
                Errors = null
            };

            if (string.IsNullOrEmpty(request.PostCode))
            {
                response = GetErrorResponse(errorList, ErrorCodes.PostCodeMissing);
            }

            if (string.IsNullOrEmpty(request.CountryCode))
            {
                response = GetErrorResponse(errorList, ErrorCodes.CountryCodeMissing);
            }
            else
            {
                if (request.CountryCode != Convert.ToString(Country.GB) & request.CountryCode != Convert.ToString(Country.US))
                {
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidCountryCode);
                }
            }

            if (string.IsNullOrEmpty(request.AddressLine1))
            {
                response = GetErrorResponse(errorList, ErrorCodes.AddressLine1Missing);
            }

            return response;
        }

        private static Response<string> GetErrorResponse(List<ErrorDetails> errorList, string errorCode)
        {
            errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());

            return new Response<string>
            {
                Status = Status.BusinessException,
                Errors = errorList
            };
        }
    }
}
