using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using static VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Triggers.Http
{
    public class EnrolmentController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnrolmentController> _logger;
        public EnrolmentController(IMediator mediator, ILogger<EnrolmentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [OpenApiOperation(operationId: "Enrolment", tags: new[] { "Enrolment Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(EnrolmentEventRequest), Required = true)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("Enrolment")]
        public async Task<HttpResponseData> Enrolment([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData httpRequest,
            FunctionContext executionContext)
        {
            var channelID = httpRequest?.Headers.GetValues("channelId").FirstOrDefault();
            var request = await new StreamReader(httpRequest.Body).ReadToEndAsync();

            EnrolmentEventRequest memberEnrolmentRequest = Json.Deserialize<EnrolmentEventRequest>(request);
            _logger.LogInformation($"Enrolment Microservice Controller - Correlation id: {memberEnrolmentRequest.Id} ChannelID : {channelID}");
            /////// Curentlly there is no field for EnrollmentChannel property for basic type . Same need to be taken as TechDebt 
            if (memberEnrolmentRequest.Data.CustomerDetails.CustomerType != CustomerTypeEnum.Basic)
            {
                memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel = channelID;
            }
            else
            {
                memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember = new Domain.CustomerDetails.LoyaltyMember() { EnrollmentChannel = channelID };
            }

            if (httpRequest.Headers.Any(k => string.Equals(k.Key, "channeluserid")))
            {
                memberEnrolmentRequest.Data.ChannelUserId = httpRequest?.Headers?.GetValues("channelUserId")?.FirstOrDefault();
            }
            else
            {
                memberEnrolmentRequest.Data.ChannelUserId = null;
            }

            return await ResultExtention<string>.ExecuteResultAsync(httpRequest, await _mediator.Send(memberEnrolmentRequest));
        }

        [OpenApiOperation(operationId: "BasicEnrolment", tags: new[] { "Enrolment Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(EnrolmentRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(string), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("BasicEnrolment")]
        public async Task<HttpResponseData> BasicEnrolment([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData httpRequest)
        {
            var request = await new StreamReader(httpRequest.Body).ReadToEndAsync();
            return await ResultExtention<string>.ExecuteResultAsync(httpRequest, await _mediator.Send(new BasicEnrolmentCommand(httpRequest?.Headers.GetValues("channelid").FirstOrDefault(), Json.Deserialize<EnrolmentRequest>(request))));
        }

        [OpenApiOperation(operationId: "EnrolmentResponse", tags: new[] { "Enrolment Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(EnrolmentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("EnrolmentResponse")]
        public async Task<HttpResponseData> EnrolmentResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "enrolment/response/{correlationid}")] HttpRequestData req, string correlationid, FunctionContext functionContext)
        {
            var commandOutput = await _mediator.Send(new EnrolmentResponseQuery { CorrelationId = correlationid, Type = Constants.Application.CreateCustomerEventSubject });
            return await ResultExtention<EnrolmentResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
