using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Constants;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Triggers.Http
{
    public class CustomerBookingController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerBookingController> _logger;
        public CustomerBookingController(IMediator mediator, ILogger<CustomerBookingController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetBooking", tags: new[] { "Order Search Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "FCNumber", In = ParameterLocation.Header, Type = typeof(string), Description = "This field specifies the Unique FCNumber.")]
        [OpenApiParameter(name: "PhoneNo", In = ParameterLocation.Header, Type = typeof(string), Description = "This field specifies the Unique Phone number.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(BookingResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetBooking")]
        public async Task<HttpResponseData> GetBooking([HttpTrigger(AuthorizationLevel.Anonymous, "GET")] HttpRequestData req)
        {
            var fCNumber = req?.Headers.GetValues("fCNumber").FirstOrDefault();
            var phoneNo = req?.Headers.GetValues("phoneNo").FirstOrDefault();

            Result<Response<BookingResponse>> result;

            try
            {
                BookingResponse response = await _mediator.Send(new LastNextBookingQuery(fCNumber, phoneNo));
                result = Result.Success(new Response<BookingResponse>
                {
                    Status = Status.OK,
                    Value = response
                });
            }
            catch (ArgumentNullException)
            {
                result = Result.Success(new Response<BookingResponse>
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MissingFCNumberAndPhoneNo), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                    Status = Status.BadRequest
                });
            }
            return await ResultExtention<BookingResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "FutureBookings", tags: new[] { "Order Search Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "FCNumber", In = ParameterLocation.Header, Type = typeof(string), Description = "This field specifies the Unique FCNumber.")]
        [OpenApiParameter(name: "PhoneNo", In = ParameterLocation.Header, Type = typeof(string), Description = "This field specifies the Unique Phone number.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(BookingResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("FutureBookings")]
        public async Task<HttpResponseData> FutureBookings([HttpTrigger(AuthorizationLevel.Anonymous, "GET")] HttpRequestData req)
        {
            var fCNumber = req?.Headers.GetValues("fCNumber").FirstOrDefault();
            var phoneNo = req?.Headers.GetValues("phoneNo").FirstOrDefault();
            var page = req?.Headers.GetValues("page").FirstOrDefault();
            var pageSize = req?.Headers.GetValues("pageSize").FirstOrDefault();

            Result<Response<BookingResponse>> result;

            try
            {
                BookingResponse response = await _mediator.Send(new FutureBookingQuery(fCNumber, phoneNo, page, pageSize));
                result = Result.Success(new Response<BookingResponse>
                {
                    Status = Status.OK,
                    Value = response
                });
            }
            catch (ArgumentNullException)
            {
                result = Result.Success(new Response<BookingResponse>
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MissingFCNumberAndPhoneNo), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                    Status = Status.BadRequest
                });
            }
            return await ResultExtention<BookingResponse>.ExecuteResultAsync(req, result);
        }
    }
}
