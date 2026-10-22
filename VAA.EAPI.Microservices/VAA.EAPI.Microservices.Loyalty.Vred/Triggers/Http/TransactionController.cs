using System;
using MediatR;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Domain.Entities.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.Http
{
    public class TransactionController
    {
        private readonly IMediator _mediator;

        public TransactionController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "CancelTransaction", tags: new[] { "VRED Microservice" }, Summary = "Cancel Transaction at Vred end", Description = "Cancel Transaction at Vred end", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CancelTransactionRequestCommand), Description = "Cancel Transaction")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(VredStatementResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        
        [Function("CancelTransaction")]
        public async Task<HttpResponseData> CancelTransaction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "VredTransaction/Cancel")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(Json.Deserialize<CancelTransactionRequestCommand>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CancelTransactionResponse", tags: new[] { "VRED Microservice" }, Summary = "Get cancel transaction event response", Description = "It returns cancel transaction event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TransactionQueryResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CancelTransactionResponse")]
        public async Task<HttpResponseData> CancelTransactionResponseAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cancelTransaction/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            Result<Response<TransactionQueryResponse>> commandOutput = await _mediator.Send(new TransactionResponseQuery { CorrelationId = correlationid, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CancelTransactionEvent.ToString())) });
            return await ResultExtention<TransactionQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CreateTransaction", tags: new[] { "VRED Microservice" }, Summary = "Apply a transaction at Vred", Description = "Apply an transaction", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CreateTransactionRequest), Description = "Transaction  Request")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CreateTransaction")]
        public async Task<HttpResponseData> CreateTransactionAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "VredTransaction/createTransaction")] HttpRequestData req)
        {
            var httpRequest = await new StreamReader(req.Body).ReadToEndAsync();
            var commandResponse = await _mediator.Send(new CreateTransactionRequestCommand()
            {
                Request = Json.Deserialize<CreateTransactionRequest>(httpRequest)
            });
            return await ResultExtention<string>.ExecuteResultAsync(req, commandResponse);
        }

        [OpenApiOperation(operationId: "CreateTransactionResponse", tags: new[] { "VRED Microservice" }, Summary = "Get Transaction event response", Description = "It returns transaction event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TransactionQueryResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CreateTransactionResponse")]
        public async Task<HttpResponseData> TransactionResponseAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "createTransaction/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            Result<Response<TransactionQueryResponse>> commandOutput = await _mediator.Send(new TransactionResponseQuery { CorrelationId = correlationid, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CreateTransactionEvent.ToString())) });
            return await ResultExtention<TransactionQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}