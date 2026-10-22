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
using VAA.EAPI.Microservices.Configuration.Application;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Triggers.Http
{
    public class CityCodeController
    {
        private readonly ILogger<CityCodeController> _logger;
        private readonly ICityCodeQuery _cityCodeQuery;

        public CityCodeController(ILogger<CityCodeController> logger, ICityCodeQuery cityCodeQuery)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cityCodeQuery = cityCodeQuery ?? throw new ArgumentNullException(nameof(cityCodeQuery));
        }
        [OpenApiOperation(operationId: "CityCodeLookup", tags: ["City Code Lookup"], Summary = "This endpoint returns a city code detail record for the supplied country code", Description = "This endpoint returns a single CityCodeData result for the CountryCode value provided in the request header", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "CountryCode", In = ParameterLocation.Header, Required = true, Type = typeof(string), Summary = "Country code header", Description = "The country code used to look up city code details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(CityCodeData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CityCode")]
        public async Task<HttpResponseData> CityCodeLookup([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CityCode")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var countryCode = req.Headers?.GetValues("CountryCode").FirstOrDefault();

                var commandOutput = await _cityCodeQuery.Handle(countryCode, cancellationToken);

                var result = Result.Success(new Response<CityCodeData>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<CityCodeData>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching CityCodeData");

                var result = CSharpFunctionalExtensions.Result.Failure<Response<CityCodeData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<CityCodeData>.ExecuteResultAsync(req, result);
            }
        }
    }
}
