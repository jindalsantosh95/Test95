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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita
{
    public class OrderRepriceController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderRepriceController> _logger;

        public OrderRepriceController(IMediator mediator, ILogger<OrderRepriceController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "OrderReprice", tags: new[] { "Order Reprice" }, Summary = "This endpoint reprice the selected offer", Description = "This endpoint reprice the selected offer for order change scenario", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OrderRepriceQuery), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderReprice")]
        public async Task<HttpResponseData> OrderReprice([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/reprice")] HttpRequestData req)
        {
            _logger.LogInformation("Order Reprice Request Received");
            var orderRepriceQuery = Json.Deserialize<OrderRepriceQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(orderRepriceQuery, CancellationToken.None);
            return await ResultExtention<OfferResponse>.ExecuteResultAsync(req, commandOutput);
        }

    }
}
