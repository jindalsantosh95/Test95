using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http
{
    public class LoyaltyOperationsLogicController
    {
        private readonly IMediator _mediator;

        public LoyaltyOperationsLogicController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "GenerateStatementDescriptionText", tags: new[] { "Loyalty Member Management Microservice - Generate StatementDescriptionText for a transaction" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(StatementTextRequest), Description = "Details to generate statement text")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(StatementTextResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GenerateStatementText")]
        public async Task<HttpResponseData> GenerateStatementText([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "transaction/GenerateStatementText")] HttpRequestData request)
        {
            StatementTextRequest input = Json.Deserialize<StatementTextRequest>(await new StreamReader(request.Body).ReadToEndAsync());

            return await ResultExtention<StatementTextResponse>.ExecuteResultAsync(request, await _mediator.Send(new TransactionStatementTextQuery() { StatementDescriptionRequest = input }));
        }
    }
}
