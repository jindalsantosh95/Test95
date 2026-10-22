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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForNameChange;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita
{
    public class OrderReshopNameChangeController
    {
        private readonly ILogger<OrderReshopNameChangeController> _logger;
        private readonly IReshopNameChange _reshopNameChange;
        private readonly CancellationToken _cancellationToken;
        public OrderReshopNameChangeController(IReshopNameChange reshopNameChangeQueryHandler, ILogger<OrderReshopNameChangeController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reshopNameChange = reshopNameChangeQueryHandler ?? throw new ArgumentNullException(nameof(_reshopNameChange));
            _cancellationToken = new CancellationToken();

        }

        [OpenApiOperation(operationId: "OrderReshopNameChange", tags: new[] { "Order Reshop Name Change" }, Summary = "Return the offer for name change", Description = "store the offer for name change and then return offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IReshopNameChange), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderReshopNameChange")]
        public async Task<HttpResponseData> OrderReshopNameChange([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/namechange")] HttpRequestData req)
        {
            _logger.LogInformation("Order Reshop NameChange Request Received");
            var ReshopNameChangeQuery = Json.Deserialize<ReshopRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _reshopNameChange.Handle(ReshopNameChangeQuery, _cancellationToken);

            return await ResultExtention<OfferResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
