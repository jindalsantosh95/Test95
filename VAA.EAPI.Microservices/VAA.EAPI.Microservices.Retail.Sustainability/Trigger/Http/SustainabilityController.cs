using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Retail.Sustainability.Domain;
using VAA.EAPI.Microservices.Retail.Sustainability.Application.Query;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Sustainability.Trigger.Http
{
    public sealed class SustainabilityController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SustainabilityController> _logger;

        public SustainabilityController(IMediator mediator, ILogger<SustainabilityController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "Sustainability", tags: new[] { "Sustainability" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(SustainMSRequest), Required = true, Description = "Retrieve sustainability data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<SustainMSResponse>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Sustainability")]
        public async Task<HttpResponseData> Sustainability([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Sustainability")] HttpRequestData req)
        {
            var requestData = await req.FunctionContext.GetHttpRequestDataAsync();

            var response = await _mediator.Send(new SustainabilityRequest()
            {
                Request = JsonConvert.DeserializeObject<SustainMSRequest>(new StreamReader(requestData.Body).ReadToEnd())
            });

            return await ResultExtention<IEnumerable<SustainMSResponse>>.ExecuteResultAsync(req, response);
        }
    }
}
