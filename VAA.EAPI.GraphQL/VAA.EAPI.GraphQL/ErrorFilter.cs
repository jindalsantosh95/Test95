using HotChocolate;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using HttpResponseException = VAA.EAPI.GraphQL.Common.Exceptions.HttpResponseException;

namespace VAA.EAPI.GraphQL
{
    public sealed class ErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (error.Exception is UnauthorizedAccessException exception &&
                (exception.Message.Is(nameof(HttpStatusCode.Unauthorized)) || exception.Message.Is(nameof(HttpStatusCode.Forbidden))))
            {
                var statusCode = exception.Message.Is(nameof(HttpStatusCode.Unauthorized))
                    ? HttpStatusCode.Unauthorized
                    : HttpStatusCode.Forbidden;

                return ErrorBuilder.FromError(error)
                    .SetMessage(statusCode.ToString())
                    .SetCode(((int)statusCode).ToString())
                    .SetExtension("httpStatusCode", (int)statusCode)
                    .RemoveException()
                    .Build();
            }

            IError customError = error;

            // Not authorised to read the field
            if (error.Code == "AUTH_NOT_AUTHORIZED")
            {
                customError = ErrorBuilder.New()
                                          .SetMessage(error.Message + Environment.NewLine + error.Path)
                                          .SetCode(((int)HttpStatusCode.Forbidden).ToString())
                                          .Build();
            }
            else if (error.Exception is not null)
            {
                var ex = error.Exception;

                IErrorBuilder builder;

                var errorSource = AsyncContext.Current.Context.GetOperationName();
                var errorFileData = ConfigReader.ReadAllText(Common.Constants.Application.FunctionAppDirPath, Common.Constants.Application.ErrorFilePath);
                List<Common.Entities.Error> errorsFromFile = Json.Deserialize<List<Common.Entities.Error>>(errorFileData) ?? new();

                if (!errorSource.IsNullOrWhiteSpace())
                {
                    var errorsForSource = errorsFromFile
                        .Where(e => e.ErrorSource?.Equals(errorSource, StringComparison.OrdinalIgnoreCase) == true)
                        .ToList();

                    ErrorDetails errorFound = errorsForSource.Count > 1
                        ? errorsForSource.FirstOrDefault(e => e.Code == ex.Message) ?? errorsForSource.FirstOrDefault()
                        : errorsForSource.FirstOrDefault();

                    if (errorFound != null)
                    {
                        var customErrorDetail = new ErrorDetails[]
                        {
                           new()
                           {
                             Code = errorFound?.Code,
                             DeveloperMessage = $"{ex.InnerException?.Message ?? ex.Message} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                             ErrorSource = errorFound?.ErrorSource,
                             ErrorTag = errorFound?.ErrorTag,
                             Message = errorFound?.Message ?? errorFound ?.DeveloperMessage
                           }
                        };
                        var tripContent = Json.Serialize(customErrorDetail);

                        return ErrorBuilder.New()
                                                .SetMessage(errorFound?.Message)
                                                .SetExtension("response", tripContent)
                                                .SetExtension("errorSource", errorSource)
                                                .SetCode(errorFound?.Code)
                                                .Build();
                    }
                }

                switch (ex)
                {
                    case HttpRequestException apiEx:

                        var sensitive = apiEx.Data.Contains("Sensitive") && apiEx.Data["Sensitive"] is true;
                        var content = sensitive ? "[REDACTED]" : apiEx.ReadResponseContentAsStringAsync().GetAwaiter().GetResult();

                        builder = ErrorBuilder.New().SetMessage("Request failed.");

                        if (!sensitive && AsyncContext.Current.HasChannel("WEB"))
                            if (((HttpResponseException)apiEx.InnerException).Response.RequestMessage.RequestUri.OriginalString.Contains("order/response"))
                            {
                                var apiErrorResponse = Json.Deserialize<dynamic>(content);
                                var errorDetails = Json.Deserialize<ErrorDetails[]>(apiErrorResponse.Error.ToString());
                                var deltaError = Json.Deserialize<dynamic>(errorDetails[0].Message);
                                var moreInfos = ((JArray)deltaError.GetValue("moreInfo", StringComparison.OrdinalIgnoreCase))?.OfType<JObject>();

                                var json = ConfigReader.ReadAllText(Common.Constants.Application.FunctionAppDirPath, Common.Constants.Application.ErrorFilePath);
                                List<ErrorDetails> errorConfigObject = Json.Deserialize<List<ErrorDetails>>(json) ?? new();

var code = errorConfigObject.Select(e => e.Code).FirstOrDefault(configCode => moreInfos != null && moreInfos.Any(mi => mi.GetValue("code", StringComparison.OrdinalIgnoreCase)?.ToString() == configCode)) ?? string.Empty;
var message = moreInfos?.FirstOrDefault(mi => mi.GetValue("code", StringComparison.OrdinalIgnoreCase)?.ToString() == code)?.GetValue("message", StringComparison.OrdinalIgnoreCase)?.ToString() ?? string.Empty;
                                ErrorDetails errors = errorConfigObject.FirstOrDefault(e => e.Code == code) ?? errorConfigObject.FirstOrDefault(e => e.Code == Common.Constants.Application.DeltaGenericErrorCode);

                                errorDetails = new ErrorDetails[]
                                {
                                    new()
                                    {
                                        Code = errors?.Code ?? code,
                                        DeveloperMessage = $"{message} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                                        ErrorSource = errors?.ErrorSource,
                                        ErrorTag = errors?.ErrorTag,
                                        Message = errors?.Message ?? message
                                    }
                                };

                                content = Json.Serialize(errorDetails);
                            }
                            else
                            {
                                var httpErrors = Json.Deserialize<List<ErrorDetails>>(content);

                                if (httpErrors.Any(c => c.Code == Common.Constants.Application.FCNumberUnavailableErrorCode))
                                {
                                    var errorDetailsJson = ConfigReader.ReadAllText(Common.Constants.Application.FunctionAppDirPath, Common.Constants.Application.ErrorFilePath);
                                    List<Common.Entities.Error> errorList = Json.Deserialize<List<Common.Entities.Error>>(errorDetailsJson) ?? new();
                                    ErrorDetails errorDetails = errorList.FirstOrDefault(e => e.ThirdPartyServiceErrorCode == Common.Constants.Application.FCNumberUnavailableErrorCode);

                                    var errors = new ErrorDetails[]
                                    {
                                        new()
                                        {
                                           Code = errorDetails?.Code ?? ((int)HttpStatusCode.BadRequest).ToString(),
                                           DeveloperMessage = $"{errorDetails?.DeveloperMessage} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                                           ErrorSource = errorDetails?.ErrorSource,
                                           ErrorTag = errorDetails?.ErrorTag,
                                           Message = errorDetails?.Message ?? errorDetails?.DeveloperMessage
                                        }
                                    };

                                    content = Json.Serialize(errors);
                                }
                            }

                        if (apiEx.InnerException is HttpResponseException)
                        {
                            customError = builder.SetExtension("code", ((int)apiEx.StatusCode).ToString())
                                                 .SetExtension("reason", ((HttpResponseException)apiEx.InnerException).Response.ReasonPhrase)
                                                 .SetExtension("response", content)
                                                 .Build();
                        }
                        else
                        {
                            customError = builder.SetExtension("code", HttpStatusCode.InternalServerError.ToString())
                                                 .SetExtension("reason", apiEx.Message)
                                                 .SetExtension("response", content)
                                                 .Build();
                        }

                        break;
                    case RequestValidationException valEx:

                        if (valEx?.InnerException?.Message == Common.Constants.Application.AccountMismatchErrorCode || valEx?.InnerException?.Message == Common.Constants.Application.MinorProfileLoginErrorCode)
                        {
                            var errorDetailsJson = ConfigReader.ReadAllText(Common.Constants.Application.FunctionAppDirPath, Common.Constants.Application.ErrorFilePath);
                            List<Common.Entities.Error> errorList = Json.Deserialize<List<Common.Entities.Error>>(errorDetailsJson) ?? new();
                            ErrorDetails errorDetails = errorList.FirstOrDefault(e => e.Code == valEx?.InnerException?.Message);
                            var errors = new ErrorDetails[]
                            {
                                new()
                                {
                                    Code = errorDetails?.Code ?? ((int)HttpStatusCode.BadRequest).ToString(),
                                    DeveloperMessage = $"{errorDetails?.DeveloperMessage} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                                    ErrorSource = errorDetails?.ErrorSource,
                                    ErrorTag = errorDetails?.ErrorTag,
                                    Message = errorDetails?.Message ?? errorDetails?.DeveloperMessage
                                }
                            };

                            content = Json.Serialize(errors);

                            customError = ErrorBuilder.New()
                                                      .SetMessage(ex.Message)
                                                      .SetCode(((int)HttpStatusCode.BadRequest).ToString())
                                                      .SetExtension("response", content)
                                                      .Build();
                            break;

                        }


                        builder = ErrorBuilder.New()
                                                .SetMessage(ex.Message)
                                                .SetCode(((int)HttpStatusCode.BadRequest).ToString());
                        foreach (var ve in ((RequestValidationException)ex).ValidationErrors.Distinct())
                            builder.SetExtension("validationRule", ve);

                        customError = builder.Build();

                        break;

                    case SecurityTokenException secEx:

                        int httpStatusCode = (int)HttpStatusCode.Unauthorized;
                        if (secEx.InnerException?.Message == Common.Constants.Application.MissingB2CTokenErrorCode)
                        {
                            httpStatusCode = (int)HttpStatusCode.Forbidden;
                        }
                        var errorJson = ConfigReader.ReadAllText(Common.Constants.Application.FunctionAppDirPath, Common.Constants.Application.ErrorFilePath);

                        List<Common.Entities.Error> errorObject = Json.Deserialize<List<Common.Entities.Error>>(errorJson) ?? new();
                        ErrorDetails errorToken = errorObject.FirstOrDefault(e => e.ThirdPartyServiceErrorCode == httpStatusCode.ToString());

                        var errorDetail = new ErrorDetails[]
                        {
                            new()
                            {
                                Code = errorToken?.Code ?? httpStatusCode.ToString(),
                                DeveloperMessage = $"{errorToken?.DeveloperMessage} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                                ErrorSource = errorToken?.ErrorSource,
                                ErrorTag = errorToken?.ErrorTag,
                                Message = errorToken?.Message ?? errorToken ?.DeveloperMessage
                            }
                        };

                        content = Json.Serialize(errorDetail);

                        customError = ErrorBuilder.New()
                                                  .SetMessage("Authorization token is missing/invalid")
                                                  .SetCode(httpStatusCode.ToString())
                                                  .SetExtension("response", content)
                                                  .Build();

                        break;
                    default:

                        ErrorDetails errorFound = errorsFromFile.FirstOrDefault(e => e.Code == ex.Message);
                        if (errorFound != null)
                        {
                            customError = ErrorBuilder.New()
                                                .SetMessage(errorFound?.DeveloperMessage)
                                                .SetCode(errorFound?.Code)
                                                .SetExtension("message", errorFound?.Message)
                                                .Build();
                        }
                        else
                            customError = ErrorBuilder.New()
                                                   .SetMessage($"Unhandled exception ({ex.Message})")
                                                   .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                                                   .Build();

                        break;
                }
            }

            return customError;
        }
    }
}
