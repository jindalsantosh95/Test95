using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Security;

namespace VAA.EAPI.Infrastructure.Controllers.Extensions
{
    public static class ResultExtention<T>
    {
        private const string SetCookieHeaderName = "Set-Cookie";

        public static async Task<HttpResponseData> ExecuteResultAsync(HttpRequestData request, Result<Response<T>> result, bool isXMLResponse = false)
        {
            var response = request.CreateResponse();

            if (isXMLResponse)
                response.Headers.Add(Constants.Constants.ContentType, Constants.Constants.XmlMediaType);
            else
                response.Headers.Add(Constants.Constants.ContentType, Constants.Constants.MediaType);

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
                        await WriteSuccessResponseAsync(result, response, isXMLResponse);
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
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.UnprocessableEntity;
                        break;
                    case Status.BusinessWarnings:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.UnprocessableEntity;
                        break;
                    case Status.BadRequest:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.BadRequest;
                        break;
                    case Status.Unauthorized:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        break;
                    case Status.Forbidden:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.Forbidden;
                        break;
                    case Status.Error:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        break;
                    case Status.NotFound:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.NotFound;
                        break;
                    case Status.TooManyRequests:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.TooManyRequests;
                        break;
                    case Status.BadGateway:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.BadGateway;
                        break;
                    case Status.ServiceUnavailable:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.ServiceUnavailable;
                        break;
                    case Status.GatewayTimeout:
                        await WriteErrorResponseAsync(result, response, isXMLResponse);
                        response.StatusCode = HttpStatusCode.GatewayTimeout;
                        break;
                    default:
                        await response.WriteStringAsync(Json.Serialize(result));
                        response.StatusCode = HttpStatusCode.UnprocessableEntity;
                        break;
                }
            }

            return response;
        }

        public static async Task WriteSuccessResponseAsync(Result<Response<T>> result, HttpResponseData response, bool isXMLResponse = false)
        {
            if (result.Value.Value == null) return;

            if (!isXMLResponse)
            {
                if (!result.Value.IgnoreNullValues)
                    await response.WriteStringAsync(Json.Serialize(result.Value.Value, false));
                else
                    await response.WriteStringAsync(Json.Serialize(result.Value.Value));
            }
            else
            {
                await response.WriteStringAsync(Xml.Serialize(result.Value.Value));
            }
        }

        public static async Task WriteErrorResponseAsync(Result<Response<T>> result, HttpResponseData response, bool isXMLResponse = false)
        {
            if (!isXMLResponse)
            {
                if (result.Value.Errors == null) return;

                if (!result.Value.IgnoreNullValues)
                    await response.WriteStringAsync(Json.Serialize(result.Value.Errors, false));
                else
                    await response.WriteStringAsync(Json.Serialize(result.Value.Errors));
            }
            else
            {
                if (result.Value.Value == null) return;

                await response.WriteStringAsync(Xml.Serialize(result.Value.Value));
            }
        }

        private static void BindResponseHeaderPara(HttpResponseData httpResponseData, IEnumerable<ResponseHeader> responseHeaders)
        {
            foreach (var headerPar in responseHeaders ?? Array.Empty<ResponseHeader>())
            {
                if (headerPar.Key.Equals(SetCookieHeaderName, StringComparison.OrdinalIgnoreCase))
                {
                    httpResponseData.Headers.TryAddWithoutValidation(SetCookieHeaderName, headerPar.Value);
                    continue;
                }

                httpResponseData.Headers.Add(headerPar.Key, headerPar.Value);
            }
        }
    }
}
