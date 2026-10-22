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
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class BannerController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BannerController> _logger;

        public BannerController(IMediator mediator, ILogger<BannerController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [OpenApiOperation(operationId: "CustomerTransactionBannerDocument", tags: new[] { "Customer Transactions Microservice - CustomerBannerDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CreateBannerRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Create Banner", Description = "Create Banner")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("CreateBanner")]
        public async Task<HttpResponseData> CreateCustomerTransactionBannerDocument([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "CreateBanner")] HttpRequestData request)
        {
            var req= Json.Deserialize<CreateBannerRequest>(await new StreamReader(request.Body).ReadToEndAsync());
            return await ResultExtention<string>.ExecuteResultAsync(request, await _mediator.Send(new CreateBannerCommand(req)));
        }

        [OpenApiOperation(operationId: "CreateBannerResponse", tags: new[] { "Customer Transactions Microservice - CreateBannerResponse" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Fetch Response", Description = "Fetch Response after Banner Creation")]
        [OpenApiParameter(name: "CorrelationId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        
        [Function("CreateBannerResponse")]
        public async Task<HttpResponseData> CreateBannerResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CreateBannerResponse/{CorrelationId}")] HttpRequestData req, string CorrelationId)
        {
            Result<Response<string>> result = await _mediator.Send(new CreateBannerResponseCommand(CorrelationId), CancellationToken.None);
            return await ResultExtention<string>.ExecuteResultAsync(req, result);

        }

        [OpenApiOperation(operationId: "GetBannerDetailsByCustomerId", tags: new[] { "Customer Transactions Microservice - GetBannerDetailsByCustomerId" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BannerResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "CustomerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("GetBannerDetailsByCustomerId")]
        public async Task<HttpResponseData> GetBannerDetailsByCustomerId([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "{CustomerId}/BannerDetails")] HttpRequestData request, string CustomerId)
        {
            return await ResultExtention<BannerResponse>.ExecuteResultAsync(request, await _mediator.Send(new BannerQuery() { CustomerId = CustomerId}));
        }

        [OpenApiOperation(operationId: "PatchBannerDetailsByCustomerId", tags: new[] { "Customer Transactions Microservice - PatchBannerDetailsByCustomerId" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "CustomerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("PatchBannerDetailsByCustomerId")]
        public async Task<HttpResponseData> PatchBannerDetailsByCustomerId([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{CustomerId}/UpdateBanner")] HttpRequestData request, string CustomerId)
        {
            PatchBannerCommand command = await TransactionBaseCommand.MapToCommand<PatchBannerCommand>(request, CustomerId);
            return await ResultExtention<BannerResponse>.ExecuteResultAsync(request, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "GetStatusOfBannerCommand", tags: new[] { "Customer Transactions Microservice - Banner" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BannerCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfBannerCommand")]
        public async Task<HttpResponseData> GetStatusOfBannerCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "banners/status/{requestId}")] HttpRequestData req, string requestId)
        {
            var commandOutput = await _mediator.Send(new BannerCommandStatusQuery() { RequestId = requestId });
            return await ResultExtention<BannerCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}