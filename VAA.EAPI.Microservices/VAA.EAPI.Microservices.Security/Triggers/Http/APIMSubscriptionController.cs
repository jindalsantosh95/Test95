using MediatR;
using Microsoft.AspNetCore.Mvc;
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
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Applications.Query.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription.BaseClasses;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription.Enums;

namespace VAA.EAPI.Microservices.Security.Triggers.Http
{
    public class APIMSubscriptionController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<APIMSubscriptionController> _logger;
        public APIMSubscriptionController(IMediator mediator, ILogger<APIMSubscriptionController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [OpenApiOperation(operationId: "CreateSubscription", tags: new[] { "Create Subscription" }, Summary = "This endpoint create the subscription for the user", Description = "This endpoint creates User in APIM and APIM subscription", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(SubscriptionCreateRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "channel", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "environment", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "environment", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(SubscriptionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CreateSubscriptionAPI")]
        public async Task<HttpResponseData> CreateSubscription([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "apim/subscription/{channel}/{environment}")] HttpRequestData req, string channel, string environment)
        {
            // Read in the request parameters from body
            SubscriptionCreateRequest request = Json.Deserialize<SubscriptionCreateRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            // Create the command
            SubscriptionCreateCommand command = new()
            {
                Channel = channel.ToUpper(),
                Environment = environment.ToUpper(),
                SubscriptionId = request.SubscriptionId,
                SubscriptionName = request.SubscriptionName,
                UserAccount = request.UserAccount
            };

            // Execute the command
            var commandOutput = await _mediator.Send(command, CancellationToken.None);

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [HttpGet]
        [OpenApiOperation(operationId: "GetSubscription", tags: new[] { "Get Subscription" }, Summary = "This endpoint get the APIM subscription details for the user", Description = "This endpoint get the APIM subscription details for the user", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Subscription Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "channel", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "environment", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "environment", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(SubscriptionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetSubscriptionAPI")]
        public async Task<HttpResponseData> GetSubscription([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apim/subscription/{channel}/{environment}/{subscriptionid}")] HttpRequestData req, string channel, string environment, string subscriptionId)
        {
            var queryResponse = await _mediator.Send(new SubscriptionRetrieveQuery { Channel = channel.ToUpper(), Environment = environment.ToUpper(), SubscriptionId = subscriptionId }, CancellationToken.None);
            return await ResultExtention<SubscriptionResponse>.ExecuteResultAsync(req, queryResponse);
        }

        [HttpPatch]
        [OpenApiOperation(operationId: "UpdateSubscription", tags: new[] { "Update Subscription" }, Summary = "This endpoint activate/suspend the subscription for the user", Description = "This endpoint activate/suspend the APIm subscription for the user", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(SubscriptionUpdateRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "channel", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "environment", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "environment", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Subscription Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(SubscriptionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateSubscriptionAPI")]
        public async Task<HttpResponseData> UpdateSubscription([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "apim/subscription/{channel}/{environment}/{subscriptionid}")] HttpRequestData req, string channel, string environment, string subscriptionId)
        {
            // Read in the request parameters from body
            SubscriptionUpdateRequest request = Json.Deserialize<SubscriptionUpdateRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            // Create the command
            SubscriptionUpdateCommand command = new()
            {
                Channel = channel.ToUpper(),
                Environment = environment.ToUpper(),
                SubscriptionId = subscriptionId,
                Enabled = Convert.ToString(request.Enabled)
            };

            // Execute the command
            var commandOutput = await _mediator.Send(command, CancellationToken.None);

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [HttpDelete]
        [OpenApiOperation(operationId: "DeleteSubscription", tags: new[] { "Delete Subscription" }, Summary = "This endpoint delete the subscription for the user", Description = "This endpoint delete the APIM subscription for the user", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "subscriptionid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Subscription Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "channel", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "environment", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "environment", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(SubscriptionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("DeleteSubscriptionAPI")]
        public async Task<HttpResponseData> DeleteSubscription([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "apim/subscription/{channel}/{environment}/{subscriptionid}")] HttpRequestData req, string channel, string environment, string subscriptionId)
        {
            // Create the command
            SubscriptionDeleteCommand command = new()
            {
                Channel = channel.ToUpper(),
                Environment = environment.ToUpper(),
                SubscriptionId = subscriptionId
            };

            // Execute the command
            var commandOutput = await _mediator.Send(command, CancellationToken.None);

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [HttpGet]
        [OpenApiOperation(operationId: "GetSubscriptionCreateResponse", tags: new[] { "Create Subscription Microservice Response" }, Summary = "Get subscription create event response", Description = "It returns subscription create event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SubscriptionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SubscriptionCreateApiResponse")]
        public async Task<HttpResponseData> SubscriptionCreateApiResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apim/subscriptioncreate/response/{correlationid}")] HttpRequestData req, string correlationId)
        {
            var commandOutput = await _mediator.Send(new SubscriptionResponseQuery { CorrelationId = correlationId, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.CreateSubscription.ToString())) });
            return await ResultExtention<SubscriptionResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [HttpGet]
        [OpenApiOperation(operationId: "GetSubscriptionUpdateResponse", tags: new[] { "Update Subscription Microservice Response" }, Summary = "Get subscription update event response", Description = "It returns subscription update event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SubscriptionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SubscriptionUpdateApiResponse")]
        public async Task<HttpResponseData> SubscriptionUpdateApiResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apim/subscriptionupdate/response/{correlationid}")] HttpRequestData req, string correlationId)
        {
            var commandOutput = await _mediator.Send(new SubscriptionResponseQuery { CorrelationId = correlationId, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.UpdateSubscription.ToString())) });
            return await ResultExtention<SubscriptionResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [HttpGet]
        [OpenApiOperation(operationId: "GetSubscriptionDeleteResponse", tags: new[] { "Delete Subscription Microservice Response" }, Summary = "Get subscription delete event response", Description = "It returns subscription delete event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SubscriptionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SubscriptionDeleteApiResponse")]
        public async Task<HttpResponseData> SubscriptionDeleteApiResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apim/subscriptiondelete/response/{correlationid}")] HttpRequestData req, string correlationId)
        {
            var commandOutput = await _mediator.Send(new SubscriptionResponseQuery { CorrelationId = correlationId, Type = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), APIMSubscriptionCommandType.DeleteSubscription.ToString())) });
            return await ResultExtention<SubscriptionResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
