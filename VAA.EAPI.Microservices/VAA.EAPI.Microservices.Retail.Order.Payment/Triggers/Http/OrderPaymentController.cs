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
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Enums;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http
{
    public class OrderPaymentController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderPaymentController> _logger;

        public OrderPaymentController(IMediator mediator, ILogger<OrderPaymentController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [OpenApiOperation(operationId: "OrderPayment", tags: new[] { "Order Payment µService" }, Summary = "Provides ability to add payment transaction details of an order in VAA system", Description = "Http endpoint to submit an event for payment update which allow asynchronous processing of payment update request in background.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<OrderPaymentRequest>), Required = true, Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "ID for Channel Used to generate payment, like Web-DF,Web-Vhols,Ndc,App-DF,CallCentre-Vhols", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "orderReference", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Unique Order ID given to customer")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("OrderPayment")]
        public async Task<HttpResponseData> OrderPayment([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "order/payment/{orderReference}")] HttpRequestData req, string orderReference)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var channel = (bool)(req?.Headers.Contains("channel")) ? (req?.Headers.GetValues("channel").FirstOrDefault()) : null;
            _logger.LogDebug($"OrderPayment Controller | Channel : {channel}");
            var requestBody = Json.Deserialize<List<OrderPaymentRequest>>(request);
            var commandOutput = await _mediator.Send(new OrderPaymentEventCommand { Channel = channel, OrderReference = orderReference, PaymentRequest = requestBody });
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }



        [OpenApiOperation(operationId: "OrderPaymentReferenceId", tags: new[] { "Order Payment µService" }, Summary = "Generates Payment Reference Id", Description = "Generates Payment Reference Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<PaymentRefIdGenerateRequest>), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentRefIdResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderPaymentReferenceId")]
        public async Task<HttpResponseData> OrderPaymentRefId([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/payment/paymentReferenceId")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var channel = AsyncContext.Current.HasChannel(ChannelType.WEB.ToString()) || AsyncContext.Current.HasChannel(ChannelType.DFMOBILE.ToString()) ? AsyncContext.Current.GetDeltaChannel() : null;
            var commandOutput = await _mediator.Send(new OrderPaymentReferenceIdGenerateCommand { Channel = channel, Request = Json.Deserialize<PaymentRefIdGenerateRequest>(request) });
            return await ResultExtention<PaymentRefIdResponse>.ExecuteResultAsync(req, commandOutput);
        }



        [OpenApiOperation(operationId: "PaymentDetails", tags: new[] { "Order Payment µService" }, Summary = "This endpoint retreives the payment details of a specific transaction.", Description = "This endpoint retreives the payment details of a specific transaction.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "formOfPayment", In = ParameterLocation.Header, Required = true, Type = typeof(PaymentType), Description = "Value to determine to call card payment details or APM formofpayment endpoint", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "paymentReferenceId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Payment Reference id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentTransactionDetails), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("PaymentDetails")]
        public async Task<HttpResponseData> PaymentDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/payment/paymentDetails/{paymentReferenceId}")] HttpRequestData req, string paymentReferenceId)
        {
            var formOfPayment = (bool)(req?.Headers.Contains("formOfPayment")) ? (req?.Headers.GetValues("formOfPayment").FirstOrDefault()) : null;
            var channel = AsyncContext.Current.HasChannel(ChannelType.WEB.ToString()) || AsyncContext.Current.HasChannel(ChannelType.DFMOBILE.ToString()) ? AsyncContext.Current.GetDeltaChannel() : null;
            var commandOutput = await _mediator.Send(new OrderPaymentDetailsCommand { Channel = channel, PaymentReferenceId = paymentReferenceId, PaymentType = (PaymentType)Enum.Parse(typeof(PaymentType), formOfPayment) });
            return await ResultExtention<PaymentTransactionDetails>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "Void", tags: new[] { "Order Payment µService" }, Summary = "Voids forms of payments.", Description = "Voids forms of payments.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(Air4formsOfPaymentsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Void")]
        public async Task<HttpResponseData> Void([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/payment/void")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(Json.Deserialize<OrderVoidCommand>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<Air4formsOfPaymentsResponse>.ExecuteResultAsync(req, commandOutput);
        }


        [OpenApiOperation(operationId: "Capture", tags: new[] { "Order Payment µService" }, Summary = "Capture forms of payments.", Description = "Capture forms of payments.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentTransactionDetails), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Capture")]
        public async Task<HttpResponseData> Capture([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/payment/capture")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new OrderPaymentCaptureCommand { Request = Json.Deserialize<PaymentCaptureRequest>(await new StreamReader(req.Body).ReadToEndAsync()) });
            return await ResultExtention<PaymentTransactionDetails>.ExecuteResultAsync(req, commandOutput);
        }

    }
}
