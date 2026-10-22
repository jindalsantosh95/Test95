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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http
{
    public class SupplementarySilverAccountController
    {
        private readonly IMediator _mediator;

        public SupplementarySilverAccountController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("GetSupplementarySilverAccountDetails")]
        [OpenApiOperation(operationId: "GetSupplementarySilverAccountDetails", tags: new[] { "Loyalty Member Management Microservice - supplementarySilver/accountDetails" }, Summary = "Submit a new request for to get Supplementary Silver account details", Description = "http endpoint to submit a request to get Supplementary Silver account details.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SupplementarySilverAccountResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> GetSupplementarySilverAccountDetailsAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            "get",
            Route = "supplementarySilver/accountDetails/{customerId}")] HttpRequestData req,
            string customerId)
        {
            var commandOutput = await _mediator.Send(new SupplementarySilverAccountDetailsQuery { CustomerId = customerId });
            return await ResultExtention<SupplementarySilverAccountResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "DelinkSupplementarySilverAccount", tags: new[] { "Loyalty Member Management Microservice - SupplementarySilver/DelinkAccount" }, Summary = "Submit a new request for Supplementary Silver Account Delinking", Description = "http endpoint to submit a request for Supplementary Silver account Delinking.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SupplementarySilverDelinkingRequest), Description = "Delink Account Details")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("DelinkSupplementarySilverAccount")]
        public async Task<HttpResponseData> DelinkSupplementarySilverAccountAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SupplementarySilver/DelinkAccount")] HttpRequestData req)
        {

            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var delinkRequest = Json.Deserialize<SupplementarySilverDelinkingRequest>(request);

            var commandOutput = await _mediator.Send(new SupplementarySilverDelinkingCommand()
            {

                InitiatedBy = delinkRequest.InitiatedBy,
                HeadAccount = delinkRequest.HeadAccount,
                ChildAccount = delinkRequest.ChildAccount

            });

            return await ResultExtention<SupplementarySilverDelinkingResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "LinkSupplementarySilverAccount", tags: new[] { "Loyalty Member Management Microservice - supplementarySilver/linkAccount" }, Summary = "Submit a new request for Supplementary Silver Account Linking", Description = "http endpoint to submit a request for Supplementary Silver account linking.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SupplementarySilverAccountLinkCommand), Description = "Link Account Details")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("LinkSSAccount")]
        public async Task<HttpResponseData> LinkAccountAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "supplementarySilver/linkAccount")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();

            var commandOutput = await _mediator.Send(Json.Deserialize<SupplementarySilverAccountLinkCommand>(request));

            return await ResultExtention<SupplementarySilverAccountLinkingResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}