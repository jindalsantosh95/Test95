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

namespace VAA.EAPI.Microservices.Configuration.Triggers.Http
{
    public class CorporateController
    {
        private readonly ILogger<CorporateController> _logger;
        private readonly ICorporateDetails _corporateDetails;

        public CorporateController(ILogger<CorporateController> logger, ICorporateDetails corporateDetails)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _corporateDetails = corporateDetails ?? throw new ArgumentNullException(nameof(corporateDetails));
        }

        [OpenApiOperation(operationId: "Corporate Lookup", tags: ["Corporate Lookup"], Summary = "This endpoint Returns corporate details for a given CBA_CLID", Description = "This endpoint returns TicketingDesignator, CorporateName and PartnerCarrierCode", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(CorporateData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CLID")]

        public async Task<HttpResponseData> Corporate([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CLID")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var cbaClid = req.Headers?.GetValues("CBA_CLID").FirstOrDefault();

                var output = await _corporateDetails.Handle(cbaClid, cancellationToken);

                var result = Result.Success(new Response<CorporateData>
                {
                    Status = Status.OK,
                    Value = output
                });

                return await ResultExtention<CorporateData>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching Corporate details");

                var result = Result.Failure<Response<CorporateData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                return await ResultExtention<CorporateData>.ExecuteResultAsync(req, result);
            }
        }
    }
}