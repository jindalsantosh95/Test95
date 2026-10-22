using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using System.IO;
using VAA.EAPI.Common.Serialisation;
using CSharpFunctionalExtensions;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.SFPD;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Query;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Triggers.Http
{ 
    public class LoyaltySyncController
    {
        private readonly IMediator _mediator;

        public LoyaltySyncController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "SyncSFPD", tags: new[] { "Loyalty Sync Microservice - SFPD" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(LoyaltySyncRequestData), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("SyncSFPD")]
        public async Task<HttpResponseData> SyncSFPD([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SyncSFPD")] HttpRequestData req)
        {
            SFPDSyncCommand request = Json.Deserialize<SFPDSyncCommand>(await new StreamReader(req.Body).ReadToEndAsync());           
            var commandOutput = await _mediator.Send(request);
         
            return await ResultExtention<SFPDSyncCommandResponse>.ExecuteResultAsync(req, commandOutput);           
        }

        [OpenApiOperation(operationId: "SyncSFPDResponse", tags: new[] { "Loyalty Sync Microservice" }, Summary = "Get SFPD update event response", Description = "It returns update SFPD event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SFPDSyncStatusQueryResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SyncSFPDResponse")]
        public async Task<HttpResponseData> SyncSFPDResponseAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SyncSFPD/status/{correlationid}")] HttpRequestData req, string correlationid)
        {
            Result<Response<SFPDSyncStatusQueryResponse>> commandOutput = await _mediator.Send(new SFPDSyncStatusQuery { RequestId = correlationid });
            return await ResultExtention<SFPDSyncStatusQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }

       
    }
}
