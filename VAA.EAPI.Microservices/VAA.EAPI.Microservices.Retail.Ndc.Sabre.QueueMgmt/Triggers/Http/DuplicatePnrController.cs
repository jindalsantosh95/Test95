using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Triggers.Http
{
    public class DuplicatePnrController(IPnrRepoService pnrRepoService)
    {
        private readonly IPnrRepoService _pnrRepoService = pnrRepoService ?? throw new ArgumentNullException(nameof(pnrRepoService));

        [OpenApiOperation(operationId: "duplicatePNR", tags: ["Queue Management Microservice"], Summary = "Fetch Duplicate PNR from Sabre", Description = "http endpoint to fetch Duplicate PNR from Sabre", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(DuplicatePnrRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(DuplicatePnrResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FetchDuplicatePnrs")]
        public async Task<HttpResponseData> FetchDuplicatePnrs([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();

            var duplicatePNRRequest = Json.Deserialize<DuplicatePnrRequest>(request);

            var serviceOutput = await _pnrRepoService.FetchDuplicatePnrs(duplicatePNRRequest.Queue, duplicatePNRRequest.PCC, CancellationToken.None);

            return await ResultExtention<DuplicatePnrResponse>.ExecuteResultAsync(req, serviceOutput);
        }

        [OpenApiOperation(operationId: "removeDuplicatePNR", tags: ["Queue Management Microservice"], Summary = "Remove Duplicate PNR from Sabre queue", Description = "http endpoint to remove Duplicate PNR from Sabre queue", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(RemoveDuplicatePnrRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(RemoveDuplicatePnrResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RemoveDuplicatePnrs")]
        public async Task<HttpResponseData> RemoveDuplicatePnrs([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();

            var removeRequest = Json.Deserialize<RemoveDuplicatePnrRequest>(request);

            if (removeRequest == null || removeRequest.PCC.IsNullOrWhiteSpace() || removeRequest.Pnr.IsNullOrWhiteSpace())
            {
                var guid = AsyncContext.Current.CorrelationId.ToString();

                return await ResultExtention<RemoveDuplicatePnrResponse>.ExecuteResultAsync(req,
                    Result.Success(new Response<RemoveDuplicatePnrResponse>
                    {
                        Errors =
                        [
                            new ErrorDetails
                            {
                                Id = guid,
                                Code = VAA.EAPI.Infrastructure.Constants.Constants.BadRequest,
                                ErrorSource = "Consuming Application",
                                Message = "Invalid request. PCC and PNR are required.",
                                DeveloperMessage = "Request body was null or PCC/PNR was missing/empty."
                            }
                        ],
                        Status = Status.BadRequest,
                        IgnoreNullValues = false
                    }));
            }

            var serviceOutput = await _pnrRepoService.RemoveDuplicate(removeRequest.PCC, removeRequest.Pnr, CancellationToken.None);

            return await ResultExtention<RemoveDuplicatePnrResponse>.ExecuteResultAsync(req, serviceOutput);
        }

    }
}
