using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query.FlightBookingCustomer;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.FlightCustomers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Triggers.Http
{
    public class FlightBookingCustomerController
    {
        private readonly IMediator _mediator;

        public FlightBookingCustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [OpenApiOperation(operationId: "FlightBookingCustomer", tags: new[] { "Order Search Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "orderid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Order id")]
        [OpenApiParameter(name: "Pnr", In = ParameterLocation.Header, Type = typeof(string), Description = "Booking reference number")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FlightCustomer), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("FlightBookingCustomer")]
        public async Task<HttpResponseData> FlightBookingCustomer([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/{orderid}/customers")] HttpRequestData req, string orderid, string Pnr)
        {
            var pnr = req?.Headers.GetValues("pnr").FirstOrDefault();
            var commandOutput = await _mediator.Send(new FlightBookingCustomerQuery(pnr, orderid), CancellationToken.None);
            return await ResultExtention<FlightCustomer>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
