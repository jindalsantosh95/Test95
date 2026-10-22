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
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Services.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http
{
    public sealed class CjsPaymentController
    {
        private readonly ICjsCheckoutPaymentApp _paymentApp;
        private readonly ILogger<CjsPaymentController> _logger;

        public CjsPaymentController(ICjsCheckoutPaymentApp paymentApp, ILogger<CjsPaymentController> logger)
        {
            _paymentApp = paymentApp ?? throw new ArgumentNullException(nameof(paymentApp));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(
            operationId: "CjsCheckoutPaymentApp",
            tags: new[] { "Holiday Payment µService" },
            Summary = "Calls CJS checkout payment app endpoint",
            Description = "Proxies a payment-app request to the CJS checkout API and returns the downstream response.",
            Visibility = OpenApiVisibilityType.Important)]       
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(CjsCheckoutPaymentAppRequest), Required = true, Description = "CJS checkout payment-app request payload")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(CjsCheckoutPaymentAppResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CjsCheckoutPaymentApp")]
        public async Task<HttpResponseData> PaymentApp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "holiday/payment")] HttpRequestData req,
            CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            _logger.LogDebug("CJS checkout payment-app request received. PayloadLength: {PayloadLength}", requestBody.Length);

            var result = await _paymentApp.HandleAsync(requestBody, cancellationToken);

            return await ResultExtention<CjsCheckoutPaymentAppResponse>.ExecuteResultAsync(req, result);
        }
    }
}
