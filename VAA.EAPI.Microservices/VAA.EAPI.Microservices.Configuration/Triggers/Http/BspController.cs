using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Configuration.Application;
using VAA.EAPI.Microservices.Configuration.Domain;
using Microsoft.Azure.Functions.Worker.Http;

namespace VAA.EAPI.Microservices.Configuration.Triggers.Http
{
    public class BspController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BspController> _logger;

        public BspController(IMediator mediator, ILogger<BspController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "BSP Market Currency Lookup", tags: new[] { "BSP Market Currency Lookup" }, Summary = "This endpoint returns the list of BSP market currency details present in the table", Description = "This endpoint returns the list of BSP Market Currency details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(BspMarketCurrencyData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("BspMarketCurrency")]
        public async Task<HttpResponseData> BspMarketCurrency([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BspMarketCurrency")] HttpRequestData req)
        {
            try
            {
                var countryCode = req.Headers?.GetValues("CountryCode").FirstOrDefault();

                var commandOutput = await _mediator.Send(new BspMarketCurrencyQuery() { CountryCode = countryCode }, CancellationToken.None);

                var result = CSharpFunctionalExtensions.Result.Success(new Response<BspMarketCurrencyData>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<BspMarketCurrencyData>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching BspMarketCurrency");

                var result = CSharpFunctionalExtensions.Result.Failure<Response<BspMarketCurrencyData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<BspMarketCurrencyData>.ExecuteResultAsync(req, result);
            }
        }

        [OpenApiOperation(operationId: "BSP Market Payment Lookup", tags: new[] { "BSP Market Payment Lookup" }, Summary = "This endpoint returns the list of BSP market payment details present in the table", Description = "This endpoint returns the list of BSP Market Payment details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(BspMarketPaymentData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("BspMarketPayment")]
        public async Task<HttpResponseData> BspMarketPayment([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BspMarketPayment")] HttpRequestData req)
        {
            try
            {
                var countryCode = req.Headers?.GetValues("CountryCode").FirstOrDefault();

                var commandOutput = await _mediator.Send(new BspMarketPaymentQuery() { CountryCode = countryCode }, CancellationToken.None);

                var result = CSharpFunctionalExtensions.Result.Success(new Response<BspMarketPaymentData>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<BspMarketPaymentData>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching BspMarketPayment");

                var result = CSharpFunctionalExtensions.Result.Failure<Response<BspMarketPaymentData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<BspMarketPaymentData>.ExecuteResultAsync(req, result);
            }
        }
    }
}
