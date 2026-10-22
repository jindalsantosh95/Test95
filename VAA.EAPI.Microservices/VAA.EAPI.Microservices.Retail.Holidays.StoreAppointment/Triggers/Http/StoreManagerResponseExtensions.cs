using System.Net;
using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Triggers.Http;

internal static class StoreManagerResponseExtensions
{
    public static Task<HttpResponseData> ExecuteAsync<T>(this ServiceResult<T> result, HttpRequestData request)
    {
        return result.ExecuteAsync(request, HttpStatusCode.OK);
    }

    public static async Task<HttpResponseData> ExecuteAsync<T>(this ServiceResult<T> result, HttpRequestData request, HttpStatusCode successStatusCode)
    {
        var response = await ResultExtention<T>.ExecuteResultAsync(request, result.ToResponse());

        if (result.IsSuccess && successStatusCode != HttpStatusCode.OK)
        {
            response.StatusCode = successStatusCode;
        }

        if (result.IsConflict)
        {
            response.StatusCode = HttpStatusCode.Conflict;
        }

        return response;
    }

    public static async Task<HttpResponseData> ExecuteDeleteAsync(this ServiceResult<bool> result, HttpRequestData request)
    {
        if (!result.IsSuccess)
        {
            return await result.ExecuteAsync(request);
        }

        var response = await ResultExtention<object>.ExecuteResultAsync(
            request,
            Result.Success(new Response<object> { Status = Status.OK }));
        response.StatusCode = HttpStatusCode.NoContent;

        response.Body = new MemoryStream();

        return response;
    }
}
