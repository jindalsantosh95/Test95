using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Security;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Common.Extensions
{
    public static class ResultExtention<T>
    {
        public static async Task<HttpResponseData> ExecuteResultAsync(HttpRequestData request, Result<Response<T>> result)
        {
            var response = request.CreateResponse();
            response.Headers.Add(Constants.Application.ContentType, Constants.Application.MediaType);
            SecurityHeaders.BindResponseSecurityHeaders(response);

            if (result.IsFailure)
            {
                await response.WriteStringAsync(result.Error);

                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            else
            {
                switch (result.Value?.Status)
                {
                    case Status.OK:
                        BindResponseHeaderPara(response, result.Value.HeaderParameters);
                        await WriteSuccessResponseAsync(result, response);
                        response.StatusCode = HttpStatusCode.OK;
                        break;
                    case Status.Accepted:
                        BindResponseHeaderPara(response, result.Value.HeaderParameters);
                        response.StatusCode = HttpStatusCode.Accepted;
                        break;
                    case Status.RedirectMethod:
                        BindResponseHeaderPara(response, result.Value.HeaderParameters);
                        response.StatusCode = HttpStatusCode.RedirectMethod;
                        break;
                    case Status.BusinessException:
                        await WriteErrorResponseAsync(result, response);
                        response.StatusCode = HttpStatusCode.UnprocessableEntity;
                        break;
                    case Status.BadRequest:
                        await WriteErrorResponseAsync(result, response);
                        response.StatusCode = HttpStatusCode.BadRequest;
                        break;
                    case Status.Unauthorized:
                        await WriteErrorResponseAsync(result, response);
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        break;
                    case Status.Forbidden:
                        await WriteErrorResponseAsync(result, response);
                        response.StatusCode = HttpStatusCode.Forbidden;
                        break;
                    case Status.Error:
                        await WriteErrorResponseAsync(result, response);
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        break;
                    case Status.PreConditionFailure:
                        BindResponseHeaderPara(response, result.Value.HeaderParameters);
                        await WriteErrorResponseAsync(result, response);
                        response.StatusCode = HttpStatusCode.PreconditionFailed;
                        break;
                    default:
                        await response.WriteStringAsync(Json.Serialize(result));
                        response.StatusCode = HttpStatusCode.UnprocessableEntity;
                        break;
                }
            }

            return response;
        }

        public static async Task WriteSuccessResponseAsync(Result<Response<T>> result, HttpResponseData response)
        {
            if (result.Value.Value == null) return;

            if (!result.Value.IgnoreNullValues)
                await response.WriteStringAsync(Json.Serialize(result.Value.Value, false));
            else
                await response.WriteStringAsync(Json.Serialize(result.Value.Value));
        }

        public static async Task WriteErrorResponseAsync(Result<Response<T>> result, HttpResponseData response)
        {
            if (result.Value.Errors == null) return;

            if (!result.Value.IgnoreNullValues)
                await response.WriteStringAsync(Json.Serialize(result.Value.Errors, false));
            else
                await response.WriteStringAsync(Json.Serialize(result.Value.Errors));
        }

        private static void BindResponseHeaderPara(HttpResponseData httpResponseData, IEnumerable<ResponseHeader> responseHeaders)
        {
            foreach (var headerPar in responseHeaders ?? Array.Empty<ResponseHeader>())
                httpResponseData.Headers.Add(headerPar.Key, headerPar.Value);
        }
    }
}
