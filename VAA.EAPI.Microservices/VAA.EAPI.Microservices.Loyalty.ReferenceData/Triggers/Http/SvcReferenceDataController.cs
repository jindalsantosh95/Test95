using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Triggers.Http
{
    public class SvcReferenceDataController
    {
        private readonly IProductDataQueryHandler _productDataQueryHandler;
        private readonly IPartnerProfileQueryHandler _partnerProfileQueryHandler;
        private readonly ITitleDataQueryHandler _titleDataQueryHandler;

        public SvcReferenceDataController(
            IProductDataQueryHandler productDataQueryHandler,
            IPartnerProfileQueryHandler partnerProfileQueryHandler,
            ITitleDataQueryHandler titleDataQueryHandler)
        {
            _productDataQueryHandler = productDataQueryHandler ?? throw new ArgumentNullException(nameof(productDataQueryHandler));
            _partnerProfileQueryHandler = partnerProfileQueryHandler ?? throw new ArgumentNullException(nameof(partnerProfileQueryHandler));
            _titleDataQueryHandler = titleDataQueryHandler ?? throw new ArgumentNullException(nameof(titleDataQueryHandler));
        }

        [OpenApiOperation(operationId: "ProductDetails", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference data for product details", Description = "Endpoint to fetch reference data of product details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<ProductDetails>), Summary = "Fetch VAA product details", Description = "Fetch product details for transaction creation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ProductDetails")]
        public async Task<HttpResponseData> ProductDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Products")] HttpRequestData req, CancellationToken cancellationToken)
        {
            var commandOutput = await _productDataQueryHandler.Handle(new ProductDataQuery(), cancellationToken);
            return await ResultExtention<IEnumerable<ProductDetails>>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "PartnerProfiles", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference data for partner profile", Description = "Endpoint to fetch reference data of partner profile", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<PartnerProfileDetails>), Summary = "Fetch VAA partnerdetails", Description = "Fetch partner details for transaction creation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("PartnerProfile")]
        public async Task<HttpResponseData> PartnerProfile([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PartnerProfile")] HttpRequestData req, CancellationToken cancellationToken)
        {

            var commandOutput = await _partnerProfileQueryHandler.Handle(new PartnerProfileQuery(), cancellationToken);
            return await ResultExtention<IEnumerable<PartnerProfileDetails>>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "Titles", tags: new[] { "Reference Microservice" }, Summary = "Fetch title reference data", Description = "Returns title reference data ordered by database priority.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IReadOnlyCollection<TitleDetails>), Summary = "Title reference data", Description = "Title reference data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Titles")]
        public async Task<HttpResponseData> Titles([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Titles")] HttpRequestData req, CancellationToken cancellationToken)
        {
            var queryOutput = await _titleDataQueryHandler.Handle(cancellationToken);
            return await ResultExtention<IReadOnlyCollection<TitleDetails>>.ExecuteResultAsync(req, queryOutput);
        }
    }
}
