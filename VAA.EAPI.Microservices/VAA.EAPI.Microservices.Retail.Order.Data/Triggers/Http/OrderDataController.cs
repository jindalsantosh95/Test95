using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.Http
{
    public sealed class OrderDataController(ISvcOrderQuery svcOrderQuery)
    {
        private readonly ISvcOrderQuery _svcOrderQuery = svcOrderQuery ?? throw new ArgumentNullException(nameof(svcOrderQuery));


        [OpenApiOperation(operationId: "SvcOrderSearch", tags: new[] { "OrderRetrieve" }, Summary = "Validates an SVC order by PNR and optional passenger name", Description = "Returns independent PNR, first-name, and surname validation results", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(SvcOrderLookupRequest), Required = true, Description = "Required PNR and optional passenger-name validation criteria")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(SvcOrderValidationResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Validation result", Description = "Independent validation results for the supplied PNR and passenger name")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SvcOrderSearch")]
        public async Task<HttpResponseData> SvcOrderSearch([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "svc/orders/search")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<SvcOrderLookupRequest>(bodyRequired: true);

            var result = await _svcOrderQuery.HandleAsync(request.Pnr, request.FirstName, request.LastName, req.FunctionContext.CancellationToken);

            return await ResultExtention<SvcOrderValidationResponse>.ExecuteResultAsync(req, result);
        }
    }
}
