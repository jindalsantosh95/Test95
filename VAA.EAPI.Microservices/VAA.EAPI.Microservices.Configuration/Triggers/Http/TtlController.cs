using CSharpFunctionalExtensions;
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
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Triggers.Http
{
    public class TtlController
    {
        private readonly ILogger<TtlController> _logger;
        private readonly ITtlRules _ttlRules;
        private readonly ITtlEnabledCity _ttlEnabledCity;

        public TtlController(ILogger<TtlController> logger, ITtlRules ttlRules, ITtlEnabledCity ttlEnabledCity)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ttlRules = ttlRules ?? throw new System.ArgumentNullException(nameof(ttlRules));
            _ttlEnabledCity = ttlEnabledCity ?? throw new System.ArgumentNullException(nameof(ttlEnabledCity));

        }

        [OpenApiOperation(operationId: "TTL Rules Lookup", tags: ["TTL Rules Lookup"], Summary = "This endpoint returns the list of TTL rules details present in the table", Description = "This endpoint returns the list of TTL rules details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(TtlRulesData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("TtlRules")]
        public async Task<HttpResponseData> TtlRules([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TtlRules")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var fareTypeCode = req.Headers?.GetValues("FareTypeCode").FirstOrDefault();

                var fareSubTypeCode = req.Headers?.GetValues("FareSubTypeCode").FirstOrDefault();

                var commandOutput = await _ttlRules.Handle(fareTypeCode, fareSubTypeCode, cancellationToken);

                var result = Result.Success(new Response<IEnumerable<TtlRulesData>>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<IEnumerable<TtlRulesData>>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching TTL Rules");

                var result = Result.Failure<Response<TtlRulesData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<TtlRulesData>.ExecuteResultAsync(req, result);
            }
        }

        [OpenApiOperation(operationId: "TTL Enabled City Lookup", tags: ["TTL Enabled City Lookup"], Summary = "This endpoint returns the list of TTL enabled cities present in the table", Description = "This endpoint returns the list of TTL enabled cities", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(TtlEnabledCityData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("TtlEnabledCity")]
        public async Task<HttpResponseData> TtlEnabledCity([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TtlEnabledCity")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var airportCode = req.Headers?.GetValues("AirportCode").FirstOrDefault();

                var commandOutput = await _ttlEnabledCity.Handle(airportCode, cancellationToken);

                var result = Result.Success(new Response<IEnumerable<TtlEnabledCityData>>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<IEnumerable<TtlEnabledCityData>>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching TTL Enabled City Data");

                var result = Result.Failure<Response<TtlEnabledCityData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<TtlEnabledCityData>.ExecuteResultAsync(req, result);
            }
        }

    }
}
