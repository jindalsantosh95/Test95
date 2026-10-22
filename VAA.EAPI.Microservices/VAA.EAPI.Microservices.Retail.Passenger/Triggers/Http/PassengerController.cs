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
using VAA.EAPI.Microservices.Retail.Passenger.Application.Commands;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Triggers.Http
{
    public class PassengerController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PassengerController> _logger;

        public PassengerController(IMediator mediator, ILogger<PassengerController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "AddPTCCodes", tags: new[] { "Passenger Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(AddPTCCodesRequestType), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddPTCCodesResponseType), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("AddPTCCodes")]
        public async Task<HttpResponseData> AddPTCCodes([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "passenger/addptccodes")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var addPTCCommand = Json.Deserialize<AddPTCCodeCommand>(await new StreamReader(req.Body).ReadToEndAsync());
            addPTCCommand.ChannelId = req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null;
            var commandOutput = await _mediator.Send(addPTCCommand);
            return await ResultExtention<AddPTCCodesResponseType>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "AddPhone", tags: new[] { "Passenger Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(AddPhoneRequestType), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddPhoneResponseType), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("AddPhone")]
        public async Task<HttpResponseData> AddPhone([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "passenger/addphone")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var addPhoneCommand = Json.Deserialize<AddPhoneCommand>(await new StreamReader(req.Body).ReadToEndAsync());
            addPhoneCommand.ChannelId = req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null;
            var commandOutput = await _mediator.Send(addPhoneCommand);
            return await ResultExtention<AddPhoneResponseType>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "ManageMultipleTravelDocsAndSecureFlight", tags: new[] { "Passenger Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ManageMultipleTravelDocsAndSecureFlightRequestType), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ManageMultipleTravelDocsAndSecureFlightResponseType), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("ManageMultipleTravelDocsAndSecureFlight")]
        public async Task<HttpResponseData> ManageMultipleTravelDocsAndSecureFlight([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "passenger/managemultipletraveldocsandsecureflight")] HttpRequestData req,
           FunctionContext executionContext)
        {
            var manageMultipleTravelDocsAndSecureFlightCommand = Json.Deserialize<ManageMultipleTravelDocsAndSecureFlightCommand>(await new StreamReader(req.Body).ReadToEndAsync());
            manageMultipleTravelDocsAndSecureFlightCommand.ChannelId = req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null;
            var commandOutput = await _mediator.Send(manageMultipleTravelDocsAndSecureFlightCommand);
            return await ResultExtention<ManageMultipleTravelDocsAndSecureFlightResponseType>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "ManageMultipleInfantV2", tags: new[] { "Passenger Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ManageMultipleInfantV2RequestType), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ManageMultipleInfantV2Response), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("ManageMultipleInfantV2")]
        public async Task<HttpResponseData> ManageMultipleInfantV2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "passenger/managemultipleinfantv2")] HttpRequestData req,
           FunctionContext executionContext)
        {
            var manageMultipleInfantCommand = Json.Deserialize<ManageMultipleInfantCommand>(await new StreamReader(req.Body).ReadToEndAsync());
            manageMultipleInfantCommand.ChannelId = req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null;
            var commandOutput = await _mediator.Send(manageMultipleInfantCommand);
            return await ResultExtention<ManageMultipleInfantV2Response>.ExecuteResultAsync(req, commandOutput);
        }


        [OpenApiOperation(operationId: "Add", tags: new[] { "Passenger Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(AddRequestType), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddResponseType), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("Add")]
        public async Task<HttpResponseData> Add([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "passenger/add")] HttpRequestData req,
          FunctionContext executionContext)
        {
            var addPassengerCommand = Json.Deserialize<AddPassengerCommand>(await new StreamReader(req.Body).ReadToEndAsync());
            addPassengerCommand.ChannelId = req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null;
            var commandOutput = await _mediator.Send(addPassengerCommand);
            return await ResultExtention<AddResponseType>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
