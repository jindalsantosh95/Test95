using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.ValidateUniqueId;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.ValidateUniqueId;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class ValidateUniqueIdController
    {
        private readonly IValidateUniqueIdHandler _validateUniqueIdHandler;
        private readonly ILogger<ValidateUniqueIdController> _logger;
        
        public ValidateUniqueIdController(IValidateUniqueIdHandler validateUniqueIdHandler, ILogger<ValidateUniqueIdController> logger)
        {
            _validateUniqueIdHandler = validateUniqueIdHandler ?? throw new ArgumentNullException(nameof(validateUniqueIdHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "ValidateUniqueIdForPCM", tags: new[] { "Campaign Microservice" }, Summary = "Validate campaign unique ID", Description = "Validates whether the unique ID exists and is unclaimed.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "uniqueId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Unique ID", Description = "Campaign unique ID to validate")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ValidateUniqueIdResponse), Summary = "Validation result", Description = "Validation result")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication is required or has failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "The caller does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validation", Description = "Business validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal error")]
        [Function("ValidateUniqueIdForPCM")]
        public async Task<HttpResponseData> ValidateUniqueIdForPCM([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ValidateUniqueIdForPCM")] HttpRequestData req, string uniqueId, CancellationToken cancellationToken)
        {
            var commandOutput = await _validateUniqueIdHandler.Handle(new ValidateUniqueIdCommand { UniqueId = uniqueId }, cancellationToken);
            return await ResultExtention<ValidateUniqueIdResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
