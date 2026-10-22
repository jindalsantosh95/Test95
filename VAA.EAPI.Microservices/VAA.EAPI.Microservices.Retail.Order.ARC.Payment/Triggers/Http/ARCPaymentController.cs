using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Net.Mime;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Application.Command;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Triggers.Http
{
    public class ARCPaymentController
    {
        private readonly ICheckStatusQuery _checkStatusQuery;
        private readonly ITransactionCommand _transactionCommand;

        public ARCPaymentController(ICheckStatusQuery checkStatusQuery, ITransactionCommand transactionCommand)
        {
            _checkStatusQuery = checkStatusQuery ?? throw new ArgumentNullException(nameof(checkStatusQuery));
            _transactionCommand = transactionCommand ?? throw new ArgumentNullException(nameof(transactionCommand));
        }

        [OpenApiOperation(operationId: "Check", tags: new[] { "ARC Payment Microservice" }, Summary = "Check Agent Status", Description = "Endpoint to call ARC service to check agent status", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(CheckResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Check")]
        public async Task<HttpResponseData> Check([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Check")] HttpRequestData req)
        {
            var agencyNumber = req.Query["agencyNumber"].ToString();

            var response = await _checkStatusQuery.Handle(agencyNumber);

            return await ResultExtention<CheckResponse>.ExecuteResultAsync(req, response);
        }


        [OpenApiOperation(operationId: "Transaction", tags: new[] { "ARC Payment Microservice" }, Summary = "Payment Transaction", Description = "Endpoint to call ARC service to report payment transaction", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(TransactionRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(PaymentTransactionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Transaction")]
        public async Task<HttpResponseData> Transaction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Transaction")] HttpRequestData req)
        {

            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var transactionRequest = Json.Deserialize<TransactionRequest>(request);

            var response = await _transactionCommand.Handle(transactionRequest);

            var result = new Response<PaymentTransactionResponse>
            {
                Status = Status.OK,
                Value = response
            };

            return await ResultExtention<PaymentTransactionResponse>.ExecuteResultAsync(req, Result.Success(result));

        }
    }
}