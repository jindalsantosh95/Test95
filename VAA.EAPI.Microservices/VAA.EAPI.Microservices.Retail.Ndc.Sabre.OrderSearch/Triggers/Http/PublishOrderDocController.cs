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
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Triggers.Http
{
    public class PublishOrderDocController
    {
        private readonly IMediator _mediator;

        public PublishOrderDocController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "PublishOrderDoc", tags: new[] { "Order Search Microservice" }, Summary = "Submit a new event to create Order", Description = "http endpoint to submit a event for Order create which allow asynchronous processing of order create request in background. The status information and response url are returned in the response. Response url further can be used to get the updated order if available,status information otherwise", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(UpdateOrderRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("PublishOrderDoc")]
        public async Task<HttpResponseData> PublishOrderDoc([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var result = await _mediator.Send(new UpdateOrderCommand(Json.Deserialize<UpdateOrderRequest>(await new StreamReader(req.Body).ReadToEndAsync())));

            return await ResultExtention<string>.ExecuteResultAsync(req, result);
        }
    }
}
