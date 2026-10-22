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
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.CRM.Application.Commands;
using VAA.EAPI.Microservices.CRM.Application.Query;
using VAA.EAPI.Microservices.CRM.Domain;

namespace VAA.EAPI.Microservices.CRM.Triggers.Http
{
    public class UpdatePaymentController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePaymentController> _logger;

        public UpdatePaymentController(IMediator mediator, ILogger<UpdatePaymentController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "UpdatePayment", tags: new[] { "CRM Payment Update Microservice" }, Summary = "Update Payment record in CRM against a Case number and Paymnet number with user's Bank details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(UpdatePaymentReqeustCommand), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Bad request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdatePayment")]
        public async Task<HttpResponseData> UpdatePayment([HttpTrigger(AuthorizationLevel.Anonymous, Constants.Application.HttpVerb)] HttpRequestData req, string orderid)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();

            if (ConfigReader.Get<bool>(Constants.Environment.CRMWebApiResponsiveFeature))
            {
                var updatePaymentReqeustCommand = Json.Deserialize<UpdatePaymentReqeustCommandV2>(request);

                var commandOutput = await _mediator.Send(updatePaymentReqeustCommand, CancellationToken.None);

                return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
            }
            else
            {
                var updatePaymentReqeustCommand = Json.Deserialize<UpdatePaymentReqeustCommand>(request);

                var commandOutput = await _mediator.Send(updatePaymentReqeustCommand, CancellationToken.None);

                return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
            }
        }

        [OpenApiOperation(operationId: "CRMPaymentUpdateStatusResponse", tags: new[] { "CRM Payment Update µService" }, Summary = "Provides ability to poll status of a payment update in CRM from Webform", Description = "It returns order payment event response and status against requested correlationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The correlation Id of the request")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CRMUpdatePaymentResponse")]
        public async Task<HttpResponseData> OrderPaymentStatusResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "updatepaymentstatus/{correlationid}")] HttpRequestData req, string correlationId)
        {
            var commandOutput = await _mediator.Send(new PaymentUpdateStatusResponseQuery { CorrelationId = correlationId, Type = Constants.Application.CRMPaymentUpdateEventSubject });

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
