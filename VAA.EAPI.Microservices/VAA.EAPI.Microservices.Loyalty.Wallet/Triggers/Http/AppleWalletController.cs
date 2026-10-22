using CSharpFunctionalExtensions;
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
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Triggers.Http
{
    public class AppleWalletController
    {
        private readonly IMediator _mediator;

        public AppleWalletController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("GeneratePass")]
        [OpenApiOperation(operationId: "GeneratePass", tags: new[] { "Apple Wallet Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(GeneratePassCommand), Description = "Enables to generate pk pass")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(byte[]), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> GeneratePass([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "generatePass")] HttpRequestData req)
        {
            return await ResultExtention<byte[]>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<GeneratePassCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("RegisterPass")]
        [OpenApiOperation(operationId: "RegisterPass", tags: new[] { "Apple Wallet Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(RegisterPassCommand), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(int), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> RegisterPass([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RegisterPass")] HttpRequestData req)
        {
            return await ResultExtention<int>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<RegisterPassCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));

        }

        [Function("DeletePass")]
        [OpenApiOperation(operationId: "DeletePass", tags: new[] { "Apple Wallet Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(DeletePassCommand), Description = "Enables to delete the pass from wallet")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(int), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> DeletePass([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeletePass")] HttpRequestData req)
        {
            return await ResultExtention<int>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<DeletePassCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("GetUpdatedPasses")]
        [OpenApiOperation(operationId: "GetUpdatedPasses", tags: new[] { "Apple Wallet Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "deviceId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Device ID")]
        [OpenApiParameter(name: "passTypeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Pass Type identifier")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AssociatedPasses), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> GetUpdatedPasses([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUpdatedPasses/{deviceId}/{passTypeId}")] HttpRequestData req, string deviceId, string passTypeId)
        {
            return await ResultExtention<AssociatedPasses>.ExecuteResultAsync(req, await _mediator.Send(new GetPassQuery() { DeviceId = deviceId, PassTypeId = passTypeId }, CancellationToken.None));
        }

        [Function("SendUpdatedPass")]
        [OpenApiOperation(operationId: "SendUpdatedPass", tags: new[] { "Apple Wallet Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "serialNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "serial number")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(byte[]), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> SendUpdatedPass([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "SendUpdatedPass/{serialNumber}")] HttpRequestData req, string serialNumber)
        {            
            return await ResultExtention<byte[]>.ExecuteResultAsync(req, await _mediator.Send(new SendUpdatedPassCommand() { FcNumber = serialNumber }, CancellationToken.None));
        }

        [Function("WalletNotification")]
        [OpenApiOperation(operationId: "WalletNotification", tags: new[] { "Apple Wallet Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SendPushNotificationCommand), Description = "Enables to send push notification.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> WalletNotification([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WalletNotification")] HttpRequestData req)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(new WalletEventNotificationCommand()));
        }

        [Function("WalletNotificationResponse")]
        [OpenApiOperation(operationId: "WalletNotificationResponse", tags: new[] { "Apple Wallet Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "CorrelationId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation Id")]        
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(WalletResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> WalletNotificationResponse([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "WalletNotificationResponse/{CorrelationId}")] HttpRequestData req, string CorrelationId)
        {
            Result<Response<WalletResponse>> result = await _mediator.Send(new WalletNotificationResponseCommand(CorrelationId), CancellationToken.None);
            return await ResultExtention<WalletResponse>.ExecuteResultAsync(req, result);
        }
    }
}
