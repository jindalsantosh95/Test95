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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment;
using CSharpFunctionalExtensions;
using System.Threading;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
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

        [OpenApiOperation(operationId: "Enrolment", tags: new[] { "Customer Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(EnrolmentRequestCommand), Required = true)]
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

            EnrolmentRequestCommand memberEnrolmentRequest = Json.Deserialize<EnrolmentRequestCommand>(request);


            if (memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember == null)
                memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember = new LoyaltyMember() { EnrollmentChannel = channelID };
            else
                memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel = channelID;


            if (httpRequest.Headers.Any(k => string.Equals(k.Key, "channeluserid")))
            {
                memberEnrolmentRequest.Data.ChannelUserId = httpRequest?.Headers?.GetValues("channelUserId")?.FirstOrDefault();
            }
            else
            {
                memberEnrolmentRequest.Data.ChannelUserId = null;
            }

            return await ResultExtention<EnrolmentResponse>.ExecuteResultAsync(httpRequest, await _mediator.Send(memberEnrolmentRequest));
        }

        [OpenApiOperation(operationId: "BasicEnrolment", tags: new[] { "Customer Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(EnrolmentRequestCommand), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(EnrolmentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Enrol Glass Tier Customer.", Description = "Enrol Glass Tier Customer in Loops system.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("BasicEnrolment")]
        public async Task<HttpResponseData> BasicEnrolment([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData httpRequest)
        {
            var channelID = httpRequest?.Headers.GetValues("channelId").FirstOrDefault();
            var request = await new StreamReader(httpRequest.Body).ReadToEndAsync();
            EnrolmentRequest enrolmentRequest = Json.Deserialize<EnrolmentRequest>(request);


            EnrolmentRequestCommand memberEnrolmentRequest = new EnrolmentRequestCommand { Data = enrolmentRequest };
            memberEnrolmentRequest.Id = Guid.NewGuid().ToString();


            if (memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember == null)
                memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember = new LoyaltyMember() { EnrollmentChannel = channelID };
            else
                memberEnrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel = channelID;


            if (httpRequest.Headers.Any(k => string.Equals(k.Key, "channeluserid")))
            {
                memberEnrolmentRequest.Data.ChannelUserId = httpRequest?.Headers?.GetValues("channelUserId")?.FirstOrDefault();
            }
            else
            {
                memberEnrolmentRequest.Data.ChannelUserId = null;
            }
            return await ResultExtention<EnrolmentResponse>.ExecuteResultAsync(httpRequest, await _mediator.Send(memberEnrolmentRequest));

        }

        [OpenApiOperation(operationId: "GenerateFcNumber", tags: new[] { "Customer Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(DeDupCheckCommand), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(EnrolmentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Create FcNumber and CutomerID for enrolment.", Description = "Create FcNumber and CutomerID for enrolment.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GenerateFcNumber")]
        public async Task<HttpResponseData> GenerateFcNumber([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData httpRequest)
        {
            var commandOutput = await _mediator.Send(Json.Deserialize<DeDupCheckCommand>(await new StreamReader(httpRequest.Body).ReadToEndAsync()));
            return await ResultExtention<EnrolmentResponse>.ExecuteResultAsync(httpRequest, commandOutput);
        }

        [OpenApiOperation(operationId: "VaEnrolment", tags: new[] { "Customer Microservice" }, Summary = "Enrol VAC members.", Description = "Enrol VAC member in Loops.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(VaEnrolmentCommand), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(string), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Enrol VAC member.", Description = "Enrol VAC member after successfull flight booking.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("VaEnrolment")]
        public async Task<HttpResponseData> VaEnrolment([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData httpRequest)
        {
            var commandOutput = await _mediator.Send(Json.Deserialize<VaEnrolmentCommand>(await new StreamReader(httpRequest.Body).ReadToEndAsync()));
            return await ResultExtention<string>.ExecuteResultAsync(httpRequest, commandOutput);
        }

        [OpenApiOperation(operationId: "VaEnrolmentResponse", tags: new[] { "Customer Microservice" }, Summary = "Retrieve VAC enrolment response.", Description = "Retrieve VAC enrolment response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "CorrelationId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation Id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(VaEnrolmentResponseResponse), Summary = "Enrol VAC Member", Description = "Enrol VAC Member in Loop's system")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("VaEnrolmentResponse")]
        public async Task<HttpResponseData> VaEnrolmentResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "VaEnrolmentResponse/{CorrelationId}")] HttpRequestData req, string CorrelationId)
        {
            Result<Response<VaEnrolmentResponseResponse>> result = await _mediator.Send(new VaEnrolmentResponseCommand(CorrelationId), CancellationToken.None);
            return await ResultExtention<VaEnrolmentResponseResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "UpgradeToFlyingClub", tags: new[] { "Customer Microservice" }, Summary = "Upgrade VAC member to FC.", Description = "Upgrade VAC member to FC and sync customer Info in Loops, OCDB, PSDS and VRED.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(UpgradeToFlyingClubCommand), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(string), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Upgrade VAC member", Description = "Upgrade VAC member to Flying Club Member")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpgradeToFlyingClub")]
        public async Task<HttpResponseData> UpgradeToFlyingClub([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData httpRequest)
        {
            var commandOutput = await _mediator.Send(Json.Deserialize<UpgradeToFlyingClubCommand>(await new StreamReader(httpRequest.Body).ReadToEndAsync()));
            return await ResultExtention<string>.ExecuteResultAsync(httpRequest, commandOutput);
        }
    }
}
