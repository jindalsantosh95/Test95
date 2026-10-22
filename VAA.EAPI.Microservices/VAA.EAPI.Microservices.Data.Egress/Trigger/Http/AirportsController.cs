using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Data.Egress.Application.Command;
using VAA.EAPI.Microservices.Data.Egress.Models;


namespace VAA.EAPI.Microservices.Data.Egress.Trigger.Http
{
    public class AirportsController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AirportsController> _logger;

        public AirportsController(IMediator mediator, ILogger<AirportsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "Publish", tags: new[] { "Publish" }, Summary = "Publish Airport data to service bus", Description = "Receives a JSON payload or airport data to push on to service bus.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<Airport>), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("Publish")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Airports/Publish")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            IEnumerable<Airport> airports;

            try
            {
                airports = Json.Deserialize<IEnumerable<Airport>>(requestBody);

                // Publish data to Service Bus
                _ = await _mediator.Send(new AirportsPublishCommand() { Airports = airports });  // Result is is not needed, it will either work, or throw exception

                // All good, send back Accepted
                return await ResultExtention<bool>.ExecuteResultAsync(req, Result.Success(new Response<bool>() { Status = Status.Accepted }));
            }
            catch (JsonReaderException) // Bad Json request
            {
                return await ResultExtention<bool>.ExecuteResultAsync(req, Result.Success(new Response<bool>()
                {
                    Status = Status.BadRequest,
                }));
            }

        }
    }
}
