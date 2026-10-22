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
using System.Net.Mime;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.PaymentMPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http
{
    public class PaymentMPIController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentMPIController> _logger;

        public PaymentMPIController(IMediator mediator, ILogger<PaymentMPIController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "SaveMPIData", tags: new[] { "Save MPI Data Service" }, Summary = "Provides ability to Sync MPI Data in Delta and Get referenceId", Description = "Http endpoint to Save MPi Data in Delta and get the referenceId for 3DS2.0 payments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<AuthenticationRequest>), Required = true, Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("SaveMPIData")]
        public async Task<HttpResponseData> SaveMPIData([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SaveMPIData")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new SaveMPICommand(Json.Deserialize<AuthenticationRequest>(await new StreamReader(req.Body).ReadToEndAsync())));
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
