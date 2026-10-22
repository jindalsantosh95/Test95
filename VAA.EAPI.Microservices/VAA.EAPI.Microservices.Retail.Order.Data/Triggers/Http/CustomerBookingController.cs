using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings;
using System.Linq;
using CSharpFunctionalExtensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightBookings;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.Http
{
    public class CustomerBookingController
    {
        private readonly IMediator _mediator;

        public CustomerBookingController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        [OpenApiOperation(operationId: "FutureBookings", tags: new[] { "FutureBookings" }, Summary = "This endpoint search the customer future bookings details from Order collection", Description = "This endpoint search the selected order details from OrderDB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "MemberNumber", In = ParameterLocation.Header, Type = typeof(string), Description = "This field specifies the Unique MemberNumber.")]
        [OpenApiParameter(name: "PhoneNo", In = ParameterLocation.Header, Type = typeof(string), Description = "This field specifies the Unique Phone number.")]
        [OpenApiParameter(name: "Email", In = ParameterLocation.Header, Type = typeof(string), Description = "This field specifies the Unique Email.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FlightBookingsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FutureBookings")]
        public async Task<HttpResponseData> FutureBookings([HttpTrigger(AuthorizationLevel.Anonymous, "GET")] HttpRequestData req)
        {
            var FCNumber = req?.Headers.GetValues("FCNumber").FirstOrDefault();
            var email = req?.Headers.GetValues("Email").FirstOrDefault();
            var phoneNo = req?.Headers.GetValues("PhoneNo").FirstOrDefault();

            Result<Response<FlightBookingsResponse>> result;

            FlightBookingsResponse response = await _mediator.Send(new FlightBookingsQuery { FCNumber = FCNumber, Email = email, PhoneNo = phoneNo });        
            result = Result.Success(new Response<FlightBookingsResponse>
            {
                Status = Status.OK,
                Value = response
            });

            return await ResultExtention<FlightBookingsResponse>.ExecuteResultAsync(req, result);


        }
    }
}
