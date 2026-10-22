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
using System.Transactions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class TransactionController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionController> _logger;
        public TransactionController(IMediator mediator, ILogger<TransactionController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetTransactionDocumentByCustomerID", tags: new[] { "Customer Transactions Microservice - Transaction Document" }, Summary = "Retrieve Transaction", Description = "Http enpoint to retrieve transaction for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TransactionResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetTransactionDocumentByCustomerID")]
        public async Task<HttpResponseData> GetTransactionDocumentByCustomerID([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/transaction")] HttpRequestData req, string customerID)
        {
            _logger.LogInformation("Request to Get Transaction documents");
            var commandOutput = await _mediator.Send(new TransactionQuery() { CustomerId = customerID, DocType = ConfigReader.Get(EnvironmentSettings.TransactionDocType) });
            return await ResultExtention<TransactionResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CreateTransaction", tags: new[] { "Customer Transactions Microservice - Transaction" }, Summary = "Create Transaction", Description ="Http enpoint to create a transaction for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TransactionDocument), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CreateTransaction")]
        public async Task<HttpResponseData> CreateTransaction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/transaction")] HttpRequestData req, string customerID)
        {
            TransactionCreateCommand command = await TransactionBaseCommand.MapToCommand<TransactionCreateCommand>(req, customerID);
            return await ResultExtention<TransactionCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "GetStatusOfTransactionCommand", tags: new[] { "Customer Transactions Microservice - Transactions" }, Summary = "Retrieve status of Create Transaction", Description = "Http enpoint to retrieve status of create transaction", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TransactionCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfTransactionCommand")]
        public async Task<HttpResponseData> GetStatusOfTransactionCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "transactions/status/{requestId}")] HttpRequestData req, string requestId)
        {
            var commandOutput = await _mediator.Send(new TransactionCommandStatusQuery() { RequestId = requestId });
            return await ResultExtention<TransactionCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CancelTransaction", tags: new[] { "Customer Transactions Microservice - Cancel Transaction" }, Summary = "Cancel Transaction", Description = "Http enpoint to cancel a transaction for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TransactionDocument), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CancelTransaction")]
        public async Task<HttpResponseData> CancelTransaction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/transaction/cancel")] HttpRequestData req, string customerID)
        {
            TransactionCancelCommand command = await TransactionBaseCommand.MapToCommand<TransactionCancelCommand>(req, customerID);
            return await ResultExtention<TransactionCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "GetRelatedTransactionsToCancel", tags: new[] { "Customer Transactions Microservice - Transaction Document" }, Summary = "Retrieve status of Cancel Transaction", Description = "Http enpoint to retrieve status of cancel transaction", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TransactionResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiParameter(name: "transactionId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Transaction Id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetRelatedTransactionsToCancel")]
        public async Task<HttpResponseData> GetRelatedTransactionsToCancel([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/transaction/cancel/RealtedTxns/{transactionId}")] HttpRequestData req, string customerID, string transactionId)
        {
            var commandOutput = await _mediator.Send(new RelatedTransactionQuery() { CustomerId = customerID, DocType = ConfigReader.Get(EnvironmentSettings.TransactionDocType), TransactionId = transactionId });
            return await ResultExtention<TransactionResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
