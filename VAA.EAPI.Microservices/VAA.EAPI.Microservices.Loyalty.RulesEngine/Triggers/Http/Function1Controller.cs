using MediatR;
using Microsoft.Extensions.Logging;
using System;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.Http
{
    public class AwardsController
    {

        private readonly IMediator _mediator;
        private readonly ILogger<AwardsController> _logger;
        public AwardsController(IMediator mediator, ILogger<AwardsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /*
         * 
         * SAMPLE FUNCTION
         * 
         * 
        [OpenApiOperation(operationId: "GetAwardsByCustID", tags: new[] { "Customer Transactions Microservice - Awards" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AwardResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetAwardsByCustID")]
        public async Task<HttpResponseData> GetMemberAwardsByCustID([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/awards")] HttpRequestData req, string customerID)
        {
            _logger.LogInformation("Get list of awards for customer");
            var commandOutput = await _mediator.Send(new AwardsQuery() { CustomerId = customerID, DocType = ConfigReader.Get(Constants.EnvironmentSettings.AwardDocType) });
            return await ResultExtention<AwardResponse>.ExecuteResultAsync(req, commandOutput);
        }
        */
    }
}

