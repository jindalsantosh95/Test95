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
using VAA.Facade.WalletFacade.Application.Command;
using VAA.Facade.WalletFacade.Domain;

namespace VAA.Facade.WalletFacade.Triggers.Http
{
    public class WalletFacadeController
    {
        private readonly IMediator _mediator;

        public WalletFacadeController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "RegisterPass", tags: new[] { "WalletFacade" }, Description = "Http endpoint for register pass  ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(PushTokenRequest), Description = "Push token request payload")]
        [OpenApiParameter(name: "deviceId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Device ID")]
        [OpenApiParameter(name: "passTypeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Pass type identifier")]
        [OpenApiParameter(name: "passSerial", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Pass serial")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(int), Description = "This returns the response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RegisterPass")]
        public async Task<HttpResponseData> RegisterPass([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/devices/{deviceId}/registrations/{passTypeId}/{passSerial}")] HttpRequestData req, string deviceId, string passTypeId, string passSerial)
        {
            return await ResultExtention<int>.ExecuteResultAsync(req, await _mediator.Send(new RegisterPassCommand() { DeviceId = deviceId, PassSerial = passSerial, PassTypeId = passTypeId, PushToken = Json.Deserialize<PushTokenRequest>(await new StreamReader(req.Body).ReadToEndAsync()).PushToken }, CancellationToken.None));
        }

        [OpenApiOperation(operationId: "DeletePass", tags: new[] { "WalletFacade" }, Description = "Http endpoint for deleting the register pass  ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "deviceId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Device ID")]
        [OpenApiParameter(name: "passTypeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Pass type identifier")]
        [OpenApiParameter(name: "passSerial", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Pass serial")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(int), Description = "This returns the response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("DeletePass")]
        public async Task<HttpResponseData> DeletePass([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/devices/{deviceId}/registrations/{passTypeId}/{passSerial}")] HttpRequestData req, string deviceId, string passTypeId, string passSerial)
        {
            return await ResultExtention<int>.ExecuteResultAsync(req, await _mediator.Send(new DeletePassCommand() { DeviceId = deviceId, PassSerial = passSerial, PassTypeId = passTypeId }, CancellationToken.None));
        }

        [Function("GetUpdatedPasses")]
        [OpenApiOperation(operationId: "GetUpdatedPasses", tags: new[] { "WalletFacade" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "deviceId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Device ID")]
        [OpenApiParameter(name: "passTypeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Pass type identifier")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AssociatedPasses), Description = "This returns the response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        public async Task<HttpResponseData> GetUpdatedPasses([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/devices/{deviceId}/registrations/{passTypeId}")] HttpRequestData req, string deviceId, string passTypeId)
        {            
            return await ResultExtention<AssociatedPasses>.ExecuteResultAsync(req, await _mediator.Send(new GetPassCommand() { DeviceId = deviceId, PassTypeId = passTypeId }, CancellationToken.None));
        }

        [Function("SendUpdatedPass")]
        [OpenApiOperation(operationId: "SendUpdatedPass", tags: new[] { "WalletFacade" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "passTypeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Pass type identifier")]
        [OpenApiParameter(name: "serialNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Serial number")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(byte[]), Description = "This returns the response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        public async Task<HttpResponseData> SendUpdatedPass([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/passes/{passTypeId}/{serialNumber}")] HttpRequestData req, string passTypeId, string serialNumber)
        {
            byte[] pass = await _mediator.Send(new SendUpdatedPassCommand() { FcNumber = serialNumber }, CancellationToken.None);

            var response = req.CreateResponse(HttpStatusCode.OK);

            response.Headers.Add(EAPI.Infrastructure.Constants.Constants.ContentType, Constants.Application.MediaType);
            response.Headers.Add(Constants.Application.ContentDisposition, $"attachment; filename=\"{serialNumber}.pkpass\"");
            response.Headers.Add(Constants.Application.AccessControlHeader, Constants.Application.ContentDisposition);
            
            await response.WriteBytesAsync(pass);

            return response;
        }
    }
}