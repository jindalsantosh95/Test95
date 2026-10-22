using CSharpFunctionalExtensions;
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
using VAA.EAPI.Microservices.CRM.Content.Application;
using VAA.EAPI.Microservices.CRM.Content.Domain.SalesBanner;

namespace VAA.EAPI.Microservices.CRM.Content.Triggers.Http
{
    public sealed class SalesBannersController
    {
        private readonly SalesBannersCommandHandler _salesBannersCommandHandler;
        private readonly RefreshSalesBannersCommandHandler _refreshSalesBannersCommandHandler;

        public SalesBannersController(SalesBannersCommandHandler salesBannersCommandHandler, RefreshSalesBannersCommandHandler refreshSalesBannersCommandHandler)
        {
            _salesBannersCommandHandler = salesBannersCommandHandler ?? throw new ArgumentNullException(nameof(salesBannersCommandHandler));
            _refreshSalesBannersCommandHandler = refreshSalesBannersCommandHandler ?? throw new ArgumentNullException(nameof(refreshSalesBannersCommandHandler));
        }

        [OpenApiOperation(operationId: "SalesBanners", tags: new[] { "Content data Microservice" }, Summary = "Return all sales banners", Description = "Sales banners are maintained in AEM and cached by ContentMS. This will return all sales banners to the client to choose the appropriate one to display.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(SalesBannerFilters), Description = "List of display locations to return. Separate locations with a tilde (~).")]
        [Function("SalesBanners")]
        public async Task<HttpResponseData> SalesBanners([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SalesBanners")] HttpRequestData req)
        {
            var body = Json.Deserialize<SalesBannerFilters>(await new StreamReader(req.Body).ReadToEndAsync());

            var response = Result.Success(new Response<Dictionary<string, IEnumerable<SalesBanner>>>
            {
                Status = Status.OK,
                Value = await _salesBannersCommandHandler.Handle((SalesBannersCommand)body, System.Threading.CancellationToken.None)
            });

            return await ResultExtention<Dictionary<string, IEnumerable<SalesBanner>>>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "RefreshSalesBanners", tags: new[] { "Content data Microservice" }, Summary = "Refresh all sales banners", Description = "Sales banners are maintained in AEM and cached by ReferenceMS. This call AEM for the latest version of the sales banners and refresh the cache.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RefreshSalesBanners")]
        public async Task<HttpResponseData> RefreshSalesBanners([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RefreshSalesBanners")] HttpRequestData req)
        {
            await _refreshSalesBannersCommandHandler.Handle(System.Threading.CancellationToken.None);

            var response = Result.Success(new Response<string>
            {
                Status = Status.OK,
                Value = "OK"
            });

            return await ResultExtention<string>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "RefreshSalesBanners", tags: new[] { "Content data Microservice" }, Summary = "Refresh all sales banners", Description = "Sales banners are maintained in AEM and cached by ReferenceMS. This call AEM for the latest version of the sales banners and refresh the cache.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RefreshSalesBannersTimer")]
        public async Task RefreshSalesBannersTimer([TimerTrigger("%SalesBannerRefreshSchedule%")] TimerInfo timerInfo, FunctionContext context)
        {
            await _refreshSalesBannersCommandHandler.Handle(System.Threading.CancellationToken.None);
        }
    }
}
