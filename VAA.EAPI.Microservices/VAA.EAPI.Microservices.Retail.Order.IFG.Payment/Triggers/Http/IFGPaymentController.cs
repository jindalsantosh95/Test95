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
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Application.Command;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Triggers.Http
{
    public class IFGPaymentController
    {
        private readonly IAuthorizePaymentCommand _authorizePaymentCommand;
        private readonly IOrderPaymentCommand _orderPaymentCommand;
        private readonly IOrderRefundCommand _refundPaymentCommand;
        private readonly IOrderExchangePaymentCommand _orderExchangePaymentCommand;
        private readonly IAuthorizeCancelPaymentCommand _authorizeCancelPaymentCommand;
        private readonly CancellationToken _cancellationToken;

        public IFGPaymentController(IAuthorizePaymentCommand authorizePaymentCommand, IOrderPaymentCommand orderPaymentCommand, IOrderRefundCommand refundPaymentCommand, IOrderExchangePaymentCommand orderExchangePaymentCommand, IAuthorizeCancelPaymentCommand authorizeCancelPaymentCommand)
        {
            _authorizePaymentCommand = authorizePaymentCommand ?? throw new ArgumentNullException(nameof(_authorizePaymentCommand));
            _orderPaymentCommand = orderPaymentCommand ?? throw new ArgumentNullException(nameof(_orderPaymentCommand));
            _refundPaymentCommand = refundPaymentCommand ?? throw new ArgumentNullException(nameof(_refundPaymentCommand));
            _orderExchangePaymentCommand = orderExchangePaymentCommand ?? throw new ArgumentNullException(nameof(orderExchangePaymentCommand));
            _authorizeCancelPaymentCommand = authorizeCancelPaymentCommand ?? throw new ArgumentNullException(nameof(_authorizeCancelPaymentCommand));
            _cancellationToken = new CancellationToken();
        }

        [OpenApiOperation(operationId: "IFGAuthorizePayment", tags: ["Authorize Payment with IFG Service"], Summary = "Provides ability to initialize and authorize payment in IFG", Description = "Http endpoint to initialize and authorize payment in IFG and returns the transaction related information for cash payments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(AuthorizePaymentRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("IFGAuthorizePayment")]
        public async Task<HttpResponseData> IFGAuthorizePayment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "IFGAuthorizePayment")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var authorizePaymentRequest = Json.Deserialize<AuthorizePaymentRequest>(request);
            var commandOutput = await _authorizePaymentCommand.Handle(authorizePaymentRequest, _cancellationToken);

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "IFGPaymentOrder", tags: ["Payment Order with IFG Service"], Summary = "Provides ability to order payment in IFG", Description = "Http endpoint to order for cash payments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(AuthorizePaymentRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("IFGPaymentOrder")]
        public async Task<HttpResponseData> IFGPaymentOrder([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "IFGOrderPayment")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var orderPaymentRequest = Json.Deserialize<AuthorizePaymentRequest>(request);
            var commandOutput = await _orderPaymentCommand.Handle(orderPaymentRequest, _cancellationToken);

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "IFGPaymentOrderRefund", tags: ["Payment Refund with IFG Service"], Summary = "Provides ability to refund payment in IFG", Description = "Http endpoint to refund for cash payments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(AuthorizePaymentRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("IFGPaymentOrderRefund")]
        public async Task<HttpResponseData> IFGPaymentOrderRefund([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "IFGPaymentOrderRefund")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var orderRefundPaymentRequest = Json.Deserialize<AuthorizePaymentRequest>(request);
            var commandOutput = await _refundPaymentCommand.Handle(orderRefundPaymentRequest, _cancellationToken);

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "IFGPaymentOrderExchange", tags: ["Payment OrderExchange with IFG Service"], Summary = "Provides ability to order exchange payment in IFG", Description = "Http endpoint to exchange for cash payments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(AuthorizePaymentRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("IFGPaymentOrderExchange")]
        public async Task<HttpResponseData> IFGPaymentOrderExchange([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "IFGPaymentOrderExchange")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var orderExchangePaymentRequest = Json.Deserialize<AuthorizePaymentRequest>(request);
            var commandOutput = await _orderExchangePaymentCommand.Handle(orderExchangePaymentRequest, _cancellationToken);

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "IFGPaymentAuthorizeCancel", tags: ["Payment AuthorizeCancel with IFG Service"], Summary = "Provides ability to authorize cancel in IFG", Description = "Http endpoint to authorize cancel for cash payments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(AuthorizePaymentRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("IFGPaymentAuthorizeCancel")]
        public async Task<HttpResponseData> IFGAuthorizeCancel([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "IFGPaymentAuthorizeCancel")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var authorizeCancelPaymentRequest = Json.Deserialize<AuthorizePaymentRequest>(request);
            var commandOutput = await _authorizeCancelPaymentCommand.Handle(authorizeCancelPaymentRequest, _cancellationToken);

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
