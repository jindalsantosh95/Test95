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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http
{
    public class ValidateBookingClassController
    {
        private readonly ILogger<ValidateBookingClassController> _logger;
        private readonly IMediator _mediator;

        public ValidateBookingClassController(IMediator mediator, ILogger<ValidateBookingClassController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("Validation")]
        [OpenApiOperation(operationId: "RegisterPass", tags: new[] { "Loyalty Member Management Microservice - Validate Booking Class" }, Summary = "A new query for Booking Class Validation", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ValidateBookingClassQuery), Description = "This should contain the Request Information for the Validation.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> ValidateBooking([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Reference/ValidateBookingClass")] HttpRequestData request)
        {
            BookingClassValidationRequest requestBody = Json.Deserialize<BookingClassValidationRequest>(await new StreamReader(request.Body).ReadToEndAsync());
            var result = await _mediator.Send(new ValidateBookingClassQuery { ValidationRequest = requestBody });
            return await ResultExtention<BookingDetailsResult>.ExecuteResultAsync(request, result);
        }
    }
}
