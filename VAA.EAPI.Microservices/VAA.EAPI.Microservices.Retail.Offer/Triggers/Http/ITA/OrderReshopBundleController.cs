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
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita
{
    public class OrderReshopBundleController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderReshopBundleController> _logger;

        public OrderReshopBundleController(IMediator mediator, ILogger<OrderReshopBundleController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        [OpenApiOperation(operationId: "OrderReshopBundle", tags: new[] { "Order Reshop" }, Summary = "Return the offer for booking change", Description = "Checks the eligibility of existing booking for change and then return offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OrderReshopBundleQuery), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderReshopBundle")]
        public async Task<HttpResponseData> OrderReshopBundle([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/reshopbundle")] HttpRequestData req)
        {
            _logger.LogInformation("Order Reshop Bundle Request Received");
            var orderReshopBundleQuery = Json.Deserialize<OrderReshopBundleQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(orderReshopBundleQuery);
            return await ResultExtention<OfferResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
