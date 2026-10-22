using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAmexUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class CampaignAmexUpdateController
    {
        private readonly ICampaignAmexUpdateRequestCommand _campaignAmexUpdateRequestCommandHandler;
        public CampaignAmexUpdateController(ICampaignAmexUpdateRequestCommand campaignAmexUpdateRequestCommandHandler)
        {
            _campaignAmexUpdateRequestCommandHandler = campaignAmexUpdateRequestCommandHandler ?? throw new ArgumentNullException(nameof(campaignAmexUpdateRequestCommandHandler));
        }

        [OpenApiOperation(operationId: "PostUpdateFCnumberForPCM", tags: new[] { "Campaign Amex Update" }, Visibility = OpenApiVisibilityType.Important, Summary = "Update MembersStatusNotes", Description = "Update MembersStatusNotes for Manage Account")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CampaginAmexUpdateRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpdateFCnumberForPCM")]
        public async Task<HttpResponseData> CampaignAmexUpdate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UpdateFCnumberForPCM")] HttpRequestData req, CancellationToken cancellationToken)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var campaginAmexUpdateRequest = Json.Deserialize<CampaginAmexUpdateRequest>(request);
            var commandOutput = await _campaignAmexUpdateRequestCommandHandler.Handle(campaginAmexUpdateRequest, cancellationToken);
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
