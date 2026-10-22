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
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BulkMemberUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http
{
    public class BulkMemberUpdateController
    {
        private readonly IBulkMemberUpdateCommand _bulkMemberUpdateCommandHandler;

        public BulkMemberUpdateController(IBulkMemberUpdateCommand bulkMemberUpdateCommandHandler)
        {
            _bulkMemberUpdateCommandHandler = bulkMemberUpdateCommandHandler ?? throw new ArgumentNullException(nameof(bulkMemberUpdateCommandHandler));
        }

        [OpenApiOperation(operationId: "PostBulkMemberUpdate", tags: new[] { "Loyalty Bulk Member Update" }, Visibility = OpenApiVisibilityType.Important, Summary = "Update MembersStatusNotes", Description = "Update MembersStatusNotes for Manage Account")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(BulkMemberUpdateRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BulkMemberUpdateResponses), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("BulkMemberUpdate")]
        public async Task<HttpResponseData> BulkMemberUpdate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "BulkMemberUpdate")] HttpRequestData req, CancellationToken cancellationToken)
        {           
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var bulkMemberStatusUpdateRequest = Json.Deserialize<BulkMemberUpdateRequest>(request);
            var commandOutput = await _bulkMemberUpdateCommandHandler.Handle(bulkMemberStatusUpdateRequest, cancellationToken);
            return await ResultExtention<BulkMemberUpdateResponses>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
