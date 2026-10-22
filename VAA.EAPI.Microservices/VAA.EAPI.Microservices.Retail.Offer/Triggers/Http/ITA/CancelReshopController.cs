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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.CancelReshop;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita
{
    public class CancelReshopController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OfferController> _logger;

        public CancelReshopController(IMediator mediator, ILogger<OfferController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "Offer", tags: new[] { "Order Cancel Reshop" }, Summary = "Return the offer for Order Cancel", Description = "Checks the eligibility of existing booking for cancel and then return offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(CancelReshopRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(CancelReshopResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CancelReshop")]
        public async Task<HttpResponseData> OrderReshopBundle([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CancelReshop")] HttpRequestData req)
        {
            _logger.LogInformation("Order Cancel Reshop Received");
            var cancelReshopRequest = Json.Deserialize<CancelReshopRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(new CancelReshopQuery(cancelReshopRequest));
            return await ResultExtention<CancelReshopResponse>.ExecuteResultAsync(req, commandOutput);
        }

    }
}
