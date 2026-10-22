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
using System.Net.Mime;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http
{
    public class CPDPaymentController
    {
        private readonly IMediator _mediator;
        private readonly ISplitPnrPaymentCommand _splitPnrPaymentCommand;

        public CPDPaymentController(IMediator mediator, ISplitPnrPaymentCommand splitPnrPaymentCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _splitPnrPaymentCommand = splitPnrPaymentCommand ?? throw new ArgumentNullException(nameof(splitPnrPaymentCommand));
        }

        [OpenApiOperation(operationId: "Authorize", tags: ["Authorize Payment with CPD Service"], Summary = "Provides ability to initialize and authorize payment in CPD", Description = "Http endpoint to initialize and authorize payment in CPD and returns the transaction related information for 3DS2.0 payments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(AuthorizePaymentRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("Authorize")]
        public async Task<HttpResponseData> Authorize([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AuthorizePayment")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new AuthorizePaymentCommand() { AuthorizePaymentRequest = Json.Deserialize<AuthorizePaymentRequest>(await new StreamReader(req.Body).ReadToEndAsync()) });
            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CapturePayment", tags: ["Capture Payment with CPD Service"], Summary = "Provides ability to Capture payment in CPD", Description = "Http endpoint to capture payment in CPD and returns the transaction related information for 3DS2.0 payments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(CapturePaymentRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("CapturePayment")]
        public async Task<HttpResponseData> CapturePayment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CapturePayment")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new CapturePaymentCommand() { CapturePaymentRequest = Json.Deserialize<CapturePaymentRequest>(await new StreamReader(req.Body).ReadToEndAsync()) });
            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "RefundPayment", tags: ["Refund Payment with CPD Service"], Summary = "Provides ability to Refund payment in CPD", Description = "Http endpoint to refund payment in CPD", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(RefundPaymentRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("RefundPayment")]
        public async Task<HttpResponseData> RefundPayment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RefundPayment")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new RefundPaymentCommand() { RefundPaymentRequest = Json.Deserialize<RefundPaymentRequest>(await new StreamReader(req.Body).ReadToEndAsync()) });
            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "Payment", tags: new[] { "Payment" }, Summary = "This endpoint returns the payment documents from PaymentDB for the given order", Description = "This endpoint returns the payment documents from PaymentDB for the given order", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "orderid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Order id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<PaymentTransactionDocument>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Payment")]
        public async Task<HttpResponseData> OrderPayment([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payment/{orderid}")] HttpRequestData req, string orderid)
        {
            var result = await _mediator.Send(new PaymentDocQuery { OrderId = orderid });
            return await ResultExtention<IEnumerable<PaymentTransactionDocument>>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "SplitPnrPayment", tags: ["Split payment document for split pnr"], Summary = "Provides ability to split payment document for split pnr", Description = "Http endpoint to split payment document for split pnr", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(SplitPnrPaymentRequest), Required = true, Description = "Parent and child booking reference, orderId and splitPaxIds")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<PaymentResponse>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("SplitPnrPayment")]
        public async Task<HttpResponseData> SplitPnrPayment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SplitPnrPayment")] HttpRequestData req)
        {
            var result = await _splitPnrPaymentCommand.Handle(Json.Deserialize<SplitPnrPaymentRequest>(await new StreamReader(req.Body).ReadToEndAsync()), new System.Threading.CancellationToken());
            return await ResultExtention<IEnumerable<PaymentResponse>>.ExecuteResultAsync(req, result);
        }
    }
}
