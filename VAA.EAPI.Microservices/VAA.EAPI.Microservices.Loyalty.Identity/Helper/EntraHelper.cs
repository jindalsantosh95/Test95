using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Helper
{
    public static class EntraErrorHelper<T>
    {

        public static Response<T> CreateErrorResponse(JObject json)
        {
            if (json == null)
            {
                return CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "service_unavailable", "Entra Service unavailable");
            }

            var error = json["error"]?.ToString() ?? "service_unavailable";
            var description = json["error_description"]?.ToString() ?? "Entra service unavailable";

            var errorCodes = json["error_codes"]?.ToObject<IEnumerable<string>>();
            

            Response<T> response;

            if (errorCodes != null && errorCodes.Any())
            {
                response = new Response<T>
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(errorCodes.First(), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                    Status = Status.BusinessException,
                };

                if (response.Errors.First().Code == null)
                {
                    response = new Response<T>
                    {
                        Status = Status.BusinessException,
                        Errors = new List<ErrorDetails>
                {
                    new ErrorDetails
                    {
                        ErrorSource = "Entra Api",
                        Message = description,
                        StatusText = error,
                        Code = errorCodes.First()
                    }
                }
                    };
                }
            }
            else
            {
                response = new Response<T>
                {
                    Status = Status.BusinessException,
                    Errors = new List<ErrorDetails>
                {
                    new ErrorDetails
                    {
                        ErrorSource = "Entra Api",
                        Message = description,
                        StatusText = error,
                        Code = "unknown_error"
                    }
                }
                };
            }

            return response;
        }

        public static Response<T> CreateErrorResponse(string errorCode, string errorStatus, string errorDescription)
        {
            return new Response<T>
            {
                Status = Status.BusinessException,
                Errors = new List<ErrorDetails>
                {
                    new ErrorDetails
                    {
                        ErrorSource = "Identity Microservice Entra",
                        Message = errorDescription,
                        StatusText = errorStatus,
                        Code = errorCode
                    }
                }
            };
        }

        public static Response<T> CreateRedirectResponse()
        {
            return new Response<T>
            {
                Status = Status.BusinessException,
                Errors = new List<ErrorDetails>
                {
                    new ErrorDetails
                    {
                        ErrorSource = "Entra External Id",
                        Message = "Authentication requires redirect to web-based flow",
                        StatusText = "redirect_required",
                    }
                }
            };
        }
    }
}
