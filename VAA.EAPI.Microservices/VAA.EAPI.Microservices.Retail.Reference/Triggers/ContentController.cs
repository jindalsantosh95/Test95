using CSharpFunctionalExtensions;
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
using VAA.EAPI.Microservices.Retail.Reference.Application;
using VAA.EAPI.Microservices.Retail.Reference.Domain.SalesBanner;

namespace VAA.EAPI.Microservices.Retail.Reference.Triggers
{
    public class ContentController
    {
        private readonly IMediator _mediator;

        public ContentController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "SalesBanners", tags: new[] { "Reference data Microservice" }, Summary = "Return all sales banners", Description = "Sales banners are maintained in AEM and cached by ReferenceMS. This will return all sales banners to the client to choose the appropriate one to display.", Visibility = OpenApiVisibilityType.Important)]
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
                Value = await _mediator.Send(new SalesBannersCommand
                {
                    Filters = body,
                })
            });

            return await ResultExtention<Dictionary<string, IEnumerable<SalesBanner>>>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "RefreshSalesBanners", tags: new[] { "Reference data Microservice" }, Summary = "Refresh all sales banners", Description = "Sales banners are maintained in AEM and cached by ReferenceMS. This call AEM for the latest version of the sales banners and refresh the cache.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RefreshSalesBanners")]
        public async Task<HttpResponseData> RefreshSalesBanners([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RefreshSalesBanners")] HttpRequestData req)
        {
            await _mediator.Send(new RefreshSalesBannersCommand());

            var response = Result.Success(new Response<string>
            {
                Status = Status.OK,
                Value = "OK"
            });

            return await ResultExtention<string>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "RefreshSalesBanners", tags: new[] { "Reference data Microservice" }, Summary = "Refresh all sales banners", Description = "Sales banners are maintained in AEM and cached by ReferenceMS. This call AEM for the latest version of the sales banners and refresh the cache.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RefreshSalesBannersTimer")]
        public async Task RefreshSalesBannersTimer([TimerTrigger("%SalesBannerRefreshSchedule%")] TimerInfo timerInfo, FunctionContext context)
        {
            await _mediator.Send(new RefreshSalesBannersCommand());
        }
    }
}
