using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Infrastructure.Controllers.Extensions
{
    internal static class FunctionContextExtenstion
    {
        internal static async Task InvokeResult(this FunctionContext context, HttpResponseData response, ILogger logger, Exception ex = null)
        {
            string guid = AsyncContext.Current.CorrelationId.ToString();

            if (ex is ValidationException validationEx)
            {
                logger.LogError(ex, $"Bad Request | CorrelationID : {guid}");
                response = await ResultExtention<object>.ExecuteResultAsync(await context.GetHttpRequestDataAsync(), Result.Success(new Response<object>()
                {
                    Errors = [ new() {
                        Id = guid,
                        Code = Constants.Constants.BadRequest,
                        ErrorSource = "Consuming Application",
                        Message = ex.Message,
                        DeveloperMessage = $"Member Names: {String.Join(", ", validationEx.ValidationResult.MemberNames)} | Value: {validationEx.Value} | TimeStamp: {DateTime.UtcNow}"
                    } ],
                    Status = Status.BadRequest,
                    IgnoreNullValues = false
                }));
            }
            else
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Forbidden:
                        logger.LogError(ex, $"Forbidden | CorrelationID : {guid}");
                        response = await ResultExtention<object>.ExecuteResultAsync(await context.GetHttpRequestDataAsync(), Result.Success(new Response<object>()
                        {
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(Constants.Constants.Forbidden, guid, Constants.Constants.GlobalExceptionFilePath),
                            Status = Status.Forbidden,
                            IgnoreNullValues = false
                        }));
                        break;
                    case HttpStatusCode.Unauthorized:
                        logger.LogError(ex, $"Unauthorized | CorrelationID : {guid}");
                        response = await ResultExtention<object>.ExecuteResultAsync(await context.GetHttpRequestDataAsync(), Result.Success(new Response<object>()
                        {
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(Constants.Constants.Unauthorized, guid, Constants.Constants.GlobalExceptionFilePath),
                            Status = Status.Unauthorized,
                            IgnoreNullValues = false
                        }));
                        break;
                    case HttpStatusCode.InternalServerError:
                        logger.LogError(ex, $"Internal Server Error | CorrelationId : {guid}");
                        response = await ResultExtention<object>.ExecuteResultAsync(await context.GetHttpRequestDataAsync(), Result.Success(new Response<object>()
                        {
                            Errors = ex is null
                                ? ErrorMapper.MapBusinessExceptionToEAPIException(Constants.Constants.TechnicalError, guid, Constants.Constants.GlobalExceptionFilePath)
                                : ErrorMapper.MapCsharpExceptionToEAPIException(ex, guid, Constants.Constants.GlobalExceptionFilePath),
                            Status = Status.Error,
                            IgnoreNullValues = false
                        }));
                        break;
                }
            }

            context.SetHttpResponseData(response);
        }
    }
}
