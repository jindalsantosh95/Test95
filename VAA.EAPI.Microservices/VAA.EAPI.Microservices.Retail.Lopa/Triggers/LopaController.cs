using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Lopa.Application.Query;
using VAA.EAPI.Microservices.Retail.Lopa.Domain.Lopa;

namespace VAA.EAPI.Microservices.Retail.Lopa.Triggers
{
    public class LopaController
    {
        private readonly IMediator _mediator;

        public LopaController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        [OpenApiOperation(operationId: "Lopa", tags: new[] { "AircraftInformation Microservice" }, Summary = "Fetch information about Aircraft", Description = "Endpoint to fetch Location of Passenger Accommodations", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<SeatMap>), Summary = "Return Aircraft Details", Description = "Return Aircraft Details")]
        [OpenApiParameter(name: "fleetTypeCode", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "IATA Code for aircraft")]
        [OpenApiParameter(name: "equipmentCode", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "equipment code")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Lopa")]
        public async Task<HttpResponseData> Lopa([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{fleetTypeCode}/{equipmentCode}/Lopa")] HttpRequestData req, string fleetTypeCode, string equipmentCode)
        {
            return await ResultExtention<List<SeatMap>>.ExecuteResultAsync(req, Result.Success(new Response<List<SeatMap>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<List<SeatMap>>(await _mediator.Send(new AircraftLopaQuery
                {
                    FleetTypeCode = fleetTypeCode,
                    EquipmentCode = equipmentCode
                })),
            })); ;
        }
    }
}
