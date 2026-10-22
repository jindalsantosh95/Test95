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
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator;
namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.Http
{
    public class AirlinePointsEarnCalculatorController
    {
        private readonly IMediator _mediator;

        public AirlinePointsEarnCalculatorController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "EarnAirlineAccuralCalculator", tags: new[] { "In Rule Engine Microservice - Earn points per segment level" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(EarnAirlineAccrualCalculatorRequest), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(EarnAirlineAccrualCalculatorResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("AirlinePointsEarnCalculator")]
        public async Task<HttpResponseData> AirlinePointsEarnCalculator([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "AirlinePointsEarnCalculator")] HttpRequestData request)
        {
            var commandOutput = await _mediator.Send(new EarnAirlineAccrualCalculatorCommand(Json.Deserialize<EarnAirlineAccrualCalculatorRequest>(await new StreamReader(request.Body).ReadToEndAsync())));

            return await ResultExtention<EarnAirlineAccrualCalculatorResponse>.ExecuteResultAsync(request, commandOutput);
        }
    }
}
