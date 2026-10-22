using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Helper.Serialisation;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Extensions
{
    public class ResultExtention<T>
    {
        public static async Task<HttpResponseData> ExecuteResultAsync(HttpRequestData request, Result<Response<T>> result)
        {
            var response = request.CreateResponse();
            response.Headers.Add(Constants.Constants.ContentType, Constants.Constants.MediaType);

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
                        await response.WriteStringAsync(Json.Serialize(result.Value.Value));
                        response.StatusCode = HttpStatusCode.OK;
                        break;
                    case Status.Accepted:
                        BindResponseHeaderPara(response, result.Value.HeaderParameters);
                        await response.WriteStringAsync(Json.Serialize(result.Value.Value));
                        response.StatusCode = HttpStatusCode.Accepted;
                        break;
                    case Status.BusinessException:
                        await response.WriteStringAsync(Json.Serialize(result.Value.Errors));
                        response.StatusCode = HttpStatusCode.UnprocessableEntity;
                        break;
                    case Status.Unauthorized:
                        await response.WriteStringAsync(Json.Serialize(result.Value.Errors));
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        break;
                    case Status.Error:
                        await response.WriteAsJsonAsync(result.Value.Errors);
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        break;
                    default:
                        await response.WriteStringAsync(Json.Serialize(result));
                        response.StatusCode = HttpStatusCode.UnprocessableEntity;
                        break;
                }
            }

            return response;
        }

        private static void BindResponseHeaderPara(HttpResponseData httpResponseData, List<ResponseHeader> responseHeaders)
        {

            if (responseHeaders != null)
            {
                foreach (var headerPar in responseHeaders)
                {
                    httpResponseData.Headers.Add(headerPar.Key, headerPar.Value);
                }
            }
        }
    }
}
