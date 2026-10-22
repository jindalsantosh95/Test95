using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class AwardsController
    {

        private readonly IMediator _mediator;
        private readonly ILogger<AwardsController> _logger;
        public AwardsController(IMediator mediator, ILogger<AwardsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetAwardsByCustID", tags: new[] { "Customer Transactions Microservice - Awards" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AwardResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetAwardsByCustID")]
        public async Task<HttpResponseData> GetMemberAwardsByCustID([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/awards")] HttpRequestData req, string customerID)
        {
            _logger.LogInformation("Get list of awards for customer");
            var commandOutput = await _mediator.Send(new AwardsQuery() { CustomerId = customerID, DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType) });
            return await ResultExtention<AwardResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "AssignTopMemberAward", tags: new[] { "Customer Transactions Microservice - Awards" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(Award), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("AssignTopMemberAward")]
        public async Task<HttpResponseData> AssignTopMemberAward([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/awards/topmember")] HttpRequestData req, string customerID, string productId)
        {
            _logger.LogInformation("Assign Award to customer");
            TopMemberCommand command = await TransactionBaseCommand.MapToCommand<TopMemberCommand>(req, customerID);
            command.ProductId = ConfigReader.Get(EnvironmentSettings.TopMemberProductID);
            return await ResultExtention<AwardCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "DeleteAward", tags: new[] { "Customer Transactions Microservice - Awards" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(Award), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("DeleteAward")]
        public async Task<HttpResponseData> DeleteAward([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "{customerID}/awards")] HttpRequestData req, string customerID)
        {
            _logger.LogInformation("Request to Delete an Award");
            AwardDeleteCommand command = await TransactionBaseCommand.MapToCommand<AwardDeleteCommand>(req, customerID);
            return await ResultExtention<AwardCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "GetStatusOfAwardCommand", tags: new[] { "Customer Transactions Microservice - Awards" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AwardCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfAwardCommand")]
        public async Task<HttpResponseData> GetStatusOfAwardCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "awards/status/{requestId}")] HttpRequestData req, string requestId)
        {
            _logger.LogInformation($"Award Command Status Request {requestId}");
            var commandOutput = await _mediator.Send(new AwardCommandStatusQuery() { RequestId = requestId });
            return await ResultExtention<AwardCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CreateAward", tags: new[] { "Customer Transactions Microservice - Awards" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(Award), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CreateAward")]
        public async Task<HttpResponseData> CreateAward([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/award")] HttpRequestData req, string customerID)
        {
            AwardCreateCommand command = await TransactionBaseCommand.MapToCommand<AwardCreateCommand>(req, customerID);
            return await ResultExtention<AwardCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "PatchAward", tags: new[] { "Customer Transactions Microservice - Awards" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(Award), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PatchAward")]
        public async Task<HttpResponseData> PatchAward([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{customerID}/award")] HttpRequestData req, string customerID)
        {
            AwardPatchCommand command = await TransactionBaseCommand.MapToCommand<AwardPatchCommand>(req, customerID);
            return await ResultExtention<AwardCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }
    }
}


