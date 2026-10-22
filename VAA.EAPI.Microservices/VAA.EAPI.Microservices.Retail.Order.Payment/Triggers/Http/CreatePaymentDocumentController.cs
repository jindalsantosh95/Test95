using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.IFG;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http
{
    public class CreatePaymentDocumentController
    {
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly ICreatePaymentDocumentQuery _createPaymentDocumentQuery;
        private readonly CancellationToken _cancellationToken;

        public CreatePaymentDocumentController(ICreatePaymentDocumentCommand createPaymentDocumentCommand, ICreatePaymentDocumentQuery createPaymentDocumentQuery)
        {
            _createPaymentDocumentCommand = createPaymentDocumentCommand ?? throw new ArgumentNullException(nameof(createPaymentDocumentCommand));
            _createPaymentDocumentQuery = createPaymentDocumentQuery ?? throw new ArgumentNullException(nameof(createPaymentDocumentQuery));
            _cancellationToken = new CancellationToken();
        }

        [OpenApiOperation(operationId: "CreatePaymentDocument", tags: ["Create Payment docuemnt in the payment db"], Summary = "Provides ability to create payment document in the payment db", Description = "Http endpoint to create payment document in payment db and returns the transaction related information.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentDocRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("CreatePaymentDocument")]
        public async Task<HttpResponseData> CreatePaymentDocument([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "CreatePaymentDocument")] HttpRequestData req)
        {
            var result = await _createPaymentDocumentCommand.Handle(Json.Deserialize<PaymentDocRequest>(await new StreamReader(req.Body).ReadToEndAsync()), _cancellationToken);

            return await ResultExtention<string>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "CreatePaymentDocumentResponse", tags: ["Create Payment docuemnt in the payment db"], Summary = "Provides ability to create payment document in the payment db", Description = "Http endpoint to create payment document in payment db and returns the transaction related information.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CreatePaymentDocumentResponse")]
        public async Task<HttpResponseData> CreatePaymentDocumentResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CreatePaymentDocument/Response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _createPaymentDocumentQuery.Handle(correlationid, Constants.Application.CreatePaymentDocumentEventType, _cancellationToken);
            
            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
