using CSharpFunctionalExtensions;
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
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Triggers.Http
{
    public class FareTypeCodeController
    {
        private readonly ILogger<FareTypeCodeController> _logger;
        private readonly IitaFareTypeCodeDetails _itaFareTypeCodeDetails;

        public FareTypeCodeController(ILogger<FareTypeCodeController> logger, IitaFareTypeCodeDetails itaFareTypeCodeDetails)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _itaFareTypeCodeDetails = itaFareTypeCodeDetails ?? throw new System.ArgumentNullException(nameof(itaFareTypeCodeDetails));

        }

        [OpenApiOperation(operationId: "ITA Fare Type Code Lookup", tags: ["ITA Fare Type Code Lookup"], Summary = "This endpoint returns the list of Fare Type codes details present in the table", Description = "This endpoint returns the list of Fare Type codes details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(BspMarketCurrencyData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ITAPaxTypeCodes")]
        public async Task<HttpResponseData> ITAFareTypeCode([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ITAPaxTypeCodes")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var fareTypeCode = req.Headers?.GetValues("FareTypeCode").FirstOrDefault();

                var fareSubTypeCode = req.Headers?.GetValues("FareSubTypeCode").FirstOrDefault();

                var commandOutput = await _itaFareTypeCodeDetails.Handle(fareTypeCode, fareSubTypeCode, cancellationToken);

                var result = Result.Success(new Response<IEnumerable<ItaFareTypeCodeDbEntity>>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<IEnumerable<ItaFareTypeCodeDbEntity>>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching Fare Type Details");

                var result = Result.Failure<Response<ItaFareTypeCodeData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<ItaFareTypeCodeData>.ExecuteResultAsync(req, result);
            }
        }
    }
}
