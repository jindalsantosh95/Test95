using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Configuration.Application;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Triggers.Http
{
    public class PccCurrencyController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PccCurrencyController> _logger;

        public PccCurrencyController(IMediator mediator, ILogger<PccCurrencyController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "PCC Currency Lookup", tags: new[] { "PCC Currency Lookup" }, Summary = "This endpoint returns the list of PCC Currency details present in the table", Description = "This endpoint returns the list of PCC Currency details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(PccCurrencyData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("PccCurrency")]
        public async Task<HttpResponseData> PccCurrency([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PccCurrency")] HttpRequestData req)
        {
            try
            {
                var currencyCode = req.Headers?.GetValues("CurrencyCode").FirstOrDefault();

                var commandOutput = await _mediator.Send(new PccCurrencyQuery() { CurrencyCode = currencyCode }, CancellationToken.None);

                var result = CSharpFunctionalExtensions.Result.Success(new Response<PccCurrencyData>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<PccCurrencyData>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching PccCurrency");

                var result = CSharpFunctionalExtensions.Result.Failure<Response<PccCurrencyData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<PccCurrencyData>.ExecuteResultAsync(req, result);
            }
        }
    }
}
