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
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command.CurrencyConverter;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Triggers.Http
{
    public class PointsCurrencyConvertController
    {
        private readonly IMediator _mediator;

        public PointsCurrencyConvertController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "CurrencyConverter", tags: new[] { "Loyalty Sync Microservice" }, Summary = "Converts currency to equivalent virgin points", Description = "Converts currency to equivalent virgin points", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<PointsCurrencyConverterRequest>), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<PointsCurrencyConverterResponse>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CurrencyConverter")]
        public async Task<HttpResponseData> CurrencyConverter([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "pointsCurrencyConverter")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new CurrencyConverterCommand { Request = Json.Deserialize<IEnumerable<PointsCurrencyConverterRequest>>(await new StreamReader(req.Body).ReadToEndAsync()) });
            return await ResultExtention<IEnumerable<PointsCurrencyConverterResponse>>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
