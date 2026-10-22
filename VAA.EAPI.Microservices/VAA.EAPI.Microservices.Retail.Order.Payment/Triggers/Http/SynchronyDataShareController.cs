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
using System.Net.Mime;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http
{
    public class SynchronyDataShareController
    {
        private readonly IMediator _mediator;

        public SynchronyDataShareController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "GenerateSynchronyToken", tags: new[] { "Order Payment µService" }, Summary = "Generates the synchrony token for Apply and Buy.", Description = "This endpoint calls synchrony data share API to get the synchrony token to continue on Apply and Buy Journey.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(SynchronyTokenRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(SynchronyTokenResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GenerateSynchronyToken")]
        public async Task<HttpResponseData> GenerateSynchronyToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/payment/synchronytoken")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new GenerateSynchronyTokenCommand { TokenRequest = Json.Deserialize<SynchronyTokenRequest>(await new StreamReader(req.Body).ReadToEndAsync()) });
            return await ResultExtention<SynchronyTokenResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "GetSynchronyApplicationStatus", tags: new[] { "Order Payment µService" }, Summary = "Gets the status for synchrony card application for Apply and Buy.", Description = "This endpoint calls synchrony data share API to get the status for synchrony card application on Apply and Buy Journey.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "token", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Synchrony Token Required to get application status")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(ApplicationStatusResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetSynchronyApplicationStatus")]
        public async Task<HttpResponseData> GetSynchronyApplicationStatus([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/payment/applicationStatus/{token}")] HttpRequestData req, string token)
        {
            var commandOutput = await _mediator.Send(new GetApplicationStatusQuery { SynchronyToken = token });
            return await ResultExtention<ApplicationStatusResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
