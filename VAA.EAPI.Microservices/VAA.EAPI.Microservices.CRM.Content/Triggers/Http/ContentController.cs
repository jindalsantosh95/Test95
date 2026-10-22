using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content.Enums;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;

namespace VAA.EAPI.Microservices.CRM.Content.Triggers.Http
{
    public sealed class ContentController
    {
        private readonly IContentRequestHandler _contentRequestHandler;

        public ContentController(IContentRequestHandler contentRequestHandler)
        {
            _contentRequestHandler = contentRequestHandler ?? throw new ArgumentNullException(nameof(contentRequestHandler));
        }

        [OpenApiOperation(operationId: "GetContent", tags: new[] { "Content Microservice" }, Summary = "Returns a content fragment", Description = "Content is maintained in AEM and cached by ContentMS.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiParameter(name: "contentFragName", Required = true, Description = "The name of the AEM content fragment", Visibility = OpenApiVisibilityType.Important, In = ParameterLocation.Path, Type = typeof(string))]
        [Function("GetContent")]
        public async Task<HttpResponseData> GetContent([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Content/{contentFragName}")] HttpRequestData req, string contentFragName)
        {
            var content = await _contentRequestHandler.Handle(contentFragName, CancellationToken.None);

            var itemsList = content?.SelectToken("items")?.Select(JsonConvert.SerializeObject) ?? Enumerable.Empty<string>();

            var response = Result.Success(new Response<IEnumerable<string>>
            {
                Status = Status.OK,
                Value = itemsList
            });

            return await ResultExtention<IEnumerable<string>>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "GetHolidayContent", tags: new[] { "Content Microservice" }, Summary = "Return holiday/aem content", Description = "Content is maintained in AEM/VHOLS and cached by ContentMS.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(ContentRequest), Description = "Content request to retrieve the required information from the corresponding source")]
        [Function("GetHolidayContent")]
        public async Task<HttpResponseData> GetHolidayContent([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Content")] HttpRequestData req)
        {
            var request = await req.ReadFromJsonAsync<ContentRequest>();
            ArgumentNullException.ThrowIfNull(request);

            var contentRequest = GetContentRequest(request);

            var content = await _contentRequestHandler.Handle(contentRequest, CancellationToken.None);
            var itemsList = content?.SelectToken("items")?.Select(JsonConvert.SerializeObject) ?? Enumerable.Empty<string>();

            var response = Result.Success(new Response<IEnumerable<string>>
            {
                Status = Status.OK,
                Value = itemsList
            });

            return await ResultExtention<IEnumerable<string>>.ExecuteResultAsync(req, response);
        }

        private ContentRequest GetContentRequest(ContentRequest request)
        {
            ArgumentNullException.ThrowIfNull(request.Detail);

            var sourceParam = request.SourceType?.ToString();
            var contentFragName = request.Detail.ContentFragmentName;

            var sourceType = SourceValidation(sourceParam);

            return sourceType switch
            {
                SourceType.AEM => new ContentRequest
                {
                    SourceType = sourceType,
                    Detail = new ContentDetail
                    {
                        ContentFragmentName = contentFragName.IsNullOrWhiteSpace()
                            ? throw new ValidationException("contentFragName is required for AEM requests.")
                            : contentFragName
                    }
                },

                SourceType.VHOLS => CreateVHOLSRequest(),

                _ => throw new ArgumentException($"Unsupported SourceType: {sourceType}")
            };

            ContentRequest CreateVHOLSRequest()
            {
                var contentTypeParam = request.Detail.ContentType?.ToString();
                var contentIdParam = request.Detail.ContentId;
                var brandCodeParam = request.Detail.BrandCode?.ToString();
                var supplierNameParam = request.Detail.SupplierName?.ToLower();
                var sippCodeParam = request.Detail.SippCode;

                var contentType = contentTypeParam.IsNullOrEmpty()
                           ? throw new ValidationException("contentType is required for VHOLS requests.")
                           : Enum.Parse<ContentType>(contentTypeParam!, true);

                if (contentTypeParam == ContentType.CAR.ToString() && (string.IsNullOrWhiteSpace(contentIdParam) || contentIdParam == "0"))
                {
                    CarRequestValidations(supplierNameParam, sippCodeParam);
                }

                var brandCode = brandCodeParam.IsNullOrEmpty()
                           ? BrandType.WW
                           : Enum.Parse<BrandType>(brandCodeParam!, true);

                return new ContentRequest
                {
                    SourceType = sourceType,
                    Detail = contentType switch
                    {
                        ContentType.HOTEL => new ContentDetail
                        {
                            ContentType = contentType,
                            ContentId = contentIdParam.IsNullOrEmpty()
                                        ? throw new ValidationException("contentId is required for VHOLS requests.")
                                        : contentIdParam,
                            BrandCode = brandCode
                        },

                        ContentType.CAR => new ContentDetail
                        {
                            ContentType = contentType,
                            ContentId = (contentIdParam.IsNullOrEmpty() || contentIdParam == "0") ? $"{supplierNameParam}_{sippCodeParam}" : contentIdParam,
                            BrandCode = brandCode
                        },

                        _ => throw new ArgumentException($"Unsupported ContentType: {contentType}")
                    }
                };
            }

        }

        private SourceType SourceValidation(string? sourceParam)
        {
            var source = sourceParam.IsNullOrWhiteSpace() ? SourceType.AEM.ToString() : sourceParam;
            if (!Enum.TryParse<SourceType>(source, true, out var sourceType))
            {
                throw new ValidationException($"Unsupported sourceType: {source}");
            }
            return sourceType;
        }

        private void CarRequestValidations(string? supplierNameParam, string? sippCodeParam)
        {
            if (supplierNameParam.IsNullOrWhiteSpace() || sippCodeParam.IsNullOrWhiteSpace())
                throw new ValidationException("SupplierName and SippCode are required when ContentId is not provided.");
        }
    }
}