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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForCancellation;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita
{
    public class ReshopForCancellationController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ReshopForCancellationController> _logger;

        public ReshopForCancellationController(IMediator mediator, ILogger<ReshopForCancellationController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "CancelReshopOrder", tags: new[] { "Order Cancel Reshop" }, Summary = "Return the offer for Order Cancel", Description = "Checks the eligibility of existing booking for cancel and then return offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(ReshopRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(CancelReshopResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CancelReshopOrder")]
        public async Task<HttpResponseData> OrderReshopCancel([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/CancelReshop")] HttpRequestData req)
        {
            var cancelReshopRequest = Json.Deserialize<ReshopForCancellationQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(cancelReshopRequest);
            return await ResultExtention<CancelReshopResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
