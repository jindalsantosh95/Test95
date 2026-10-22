using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Insurance.Application.Commands;
using VAA.EAPI.Microservices.Retail.Insurance.Domain;


namespace VAA.EAPI.Microservices.Retail.Insurance.Triggers.Http
{
    public class InsuranceQuoteController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InsuranceQuoteController> _logger;

        public InsuranceQuoteController(IMediator mediator, ILogger<InsuranceQuoteController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "InsuranceQuote", tags: new[] { "Insurance" }, Summary = "Service to request and receive insurance offers", Description = "Service to request and receive insurance offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(InsuranceQuoteRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(InsuranceQuoteResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Not Found", Description = "Not Found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("InsuranceQuote")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "InsuranceQuote")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();

            var requestBody = Json.Deserialize<InsuranceQuoteRequest>(request);
            var commandOutput = await _mediator.Send(new CreateQuoteCommand { Request = requestBody });

            return await ResultExtention<InsuranceQuoteResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
