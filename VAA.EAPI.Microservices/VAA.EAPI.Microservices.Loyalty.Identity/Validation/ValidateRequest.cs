using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Validation
{
    public class ValidateRequest
    {
        public static Result<Response<string>> RequestValidator(AcquireTokenRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var errorList = new List<ErrorDetails>();
            var response = new Response<string>
            {
                Status = Status.OK,
                Errors = null
            };

            //validate FirstName
            if (string.IsNullOrEmpty(request.FirstName))
            {
                response = GetErrorResponse(errorList, ErrorCodes.FirstNameMissing);
            }
            //validate LastName
            if (string.IsNullOrEmpty(request.LastName))
            {
                response = GetErrorResponse(errorList, ErrorCodes.LastNameMissing);
            }
            if (request.Type != null && request.Type.Length > 0)
            {
                //Validating PNR and its length should be 6 characters   
                if (string.Equals(request.Type, Constants.Application.PnrType, StringComparison.OrdinalIgnoreCase) && request.RLOC != null && request.RLOC?.Length != Constants.Application.PNRLength)
                {
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidPNR);
                }
                //Validating eTicket and its length should be 13 characters
                else if (string.Equals(request.Type, Constants.Application.TicketType, StringComparison.OrdinalIgnoreCase) && request.TicketRef != null && request.TicketRef?.Length != Constants.Application.ETicketLength)
                {
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidETicketNumber);
                }
                else if (!string.Equals(request.Type, Constants.Application.TicketType, StringComparison.OrdinalIgnoreCase) && !string.Equals(request.Type, Constants.Application.PnrType, StringComparison.OrdinalIgnoreCase))
                {
                    response = GetErrorResponse(errorList, ErrorCodes.TypeMissingOrInvalid);
                }
            }
            else
            {
                response = GetErrorResponse(errorList, ErrorCodes.TypeMissingOrInvalid);
            }
            return response;
        }
        private static Response<string> GetErrorResponse(List<ErrorDetails> errorList, string errorCode)
        {
            errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
            return new Response<string>
            {
                Status = Status.Unauthorized,
                Errors = errorList
            };
        }
    }
}
