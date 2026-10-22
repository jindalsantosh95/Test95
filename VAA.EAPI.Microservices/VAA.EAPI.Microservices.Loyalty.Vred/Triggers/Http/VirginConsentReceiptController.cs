using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.Http
{

    public sealed class VirginConsentReceiptController
    {
        private readonly ILogger<VirginConsentReceiptController> logger;
        private readonly IMediator _mediator;

        public VirginConsentReceiptController(IMediator mediator, ILogger<VirginConsentReceiptController> logger)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "VirginConsentReceipt", tags: new[] { "VRED VirginConsentReceipt Microservice" }, Summary = "Set Marketing prefrence.", Description = "http endpoint to Set Marketing prefrence at VRED", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(VredConsentReceiptCommand), Description = "Send marketing preference consent to VRED")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Send marketing preference consent to VRED", Description = "Send marketing preference consent to VRED during enrolment")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("VirginConsentReceipt")]
        public async Task<HttpResponseData> VirginMarketingConsent([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "VirginConsentReceipt")] HttpRequestData req)
        {
            var consentData = Json.Deserialize<VredConsentReceiptCommand>(await new StreamReader(req.Body).ReadToEndAsync());

            var commandOutput = await _mediator.Send(consentData);

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
