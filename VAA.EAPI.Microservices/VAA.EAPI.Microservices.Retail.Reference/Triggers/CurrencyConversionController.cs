using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Reference.Application;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Triggers
{
    public class CurrencyConversionController
    {
        
        private readonly ICurrencyConversionCommand _currencyConversionCommand;
        private readonly ICurrencyExchangeRateQuery _currencyExchangeRateQuery;

        public CurrencyConversionController(ICurrencyConversionCommand currencyConversionCommand, ICurrencyExchangeRateQuery currencyExchangeRateQuery)
        {
            _currencyConversionCommand = currencyConversionCommand ?? throw new ArgumentNullException(nameof(currencyConversionCommand));
            _currencyExchangeRateQuery = currencyExchangeRateQuery ?? throw new ArgumentNullException(nameof(currencyExchangeRateQuery));
        }

        [OpenApiOperation(operationId: "CurrencyConversion", tags: new[] { "Currency Conversion" }, Summary = "CurrencyConversion", Description = "Currency Conversion endpoint to get exchange rate and store in blob", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CurrencyConversion")]
        public async Task CurrencyConversion([TimerTrigger("%CurrencyConversionRefreshSchedule%")] TimerInfo timerInfo, FunctionContext context)
        {
            await _currencyConversionCommand.Handle();
        }

        [OpenApiOperation(operationId: "CurrencyConversion", tags: new[] { "Reference data Microservice" }, Summary = "Fetch The latest exchange rate", Description = "Endpoint to fetch Fetch The latest exchange rate stored in blob", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(ExchangeRateResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CurrencyExchangeRate")]
        public async Task<HttpResponseData> CurrencyExchangeRate([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CurrencyExchangeRate")] HttpRequestData req)
        {
            var response = await _currencyExchangeRateQuery.Handle();

            var result = new Response<ExchangeRateResponse>
            {
                Status = Status.OK,
                Value = response
            };

            return await ResultExtention<ExchangeRateResponse>.ExecuteResultAsync(req, Result.Success(result));
        }

    }
}
