using CSharpFunctionalExtensions;
using MediatR;
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
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.AmexCenturion;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Award;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.LoyaltyGenericAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Preference;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Triggers.Http
{
    public class AuditHistoryController
    {
        private readonly IMediator _mediator;

        public AuditHistoryController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "PreferenceHistoryDocuments", tags: new[] { "Audit Microservice - PreferenceHistoryDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PreferenceAuditHistoryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PreferenceHistoryDocumentsByCustomerId")]
        public async Task<HttpResponseData> FetchPreferenceHistoryDocumentsByCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerId}/preferenceHistory")] HttpRequestData request, string customerId)
        {
            var result = await _mediator.Send(new PreferenceAuditHistoryQuery(customerId), cancellationToken: CancellationToken.None);
            return await ResultExtention<PreferenceAuditHistoryResponse>.ExecuteResultAsync(request,
                Result.Success(new Response<PreferenceAuditHistoryResponse>()
                {
                    Status = Status.OK,
                    Value = new PreferenceAuditHistoryResponse(result)
                }));
        }

        [OpenApiOperation(operationId: "AwardHistoryDocuments", tags: new[] { "Audit Microservice - AwardHistoryDocuments" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AwardAuditHistoryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("AwardHistoryDocumentsByCustomerId")]
        public async Task<HttpResponseData> FetchAwardHistoryDocumentsByCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerId}/awardAuditHistory")] HttpRequestData request,
            string customerId)
        {
            var result = await _mediator.Send(new AwardAuditHistoryQuery(customerId), cancellationToken: CancellationToken.None);
            return await ResultExtention<AwardAuditHistoryResponse>.ExecuteResultAsync(request,
                Result.Success(new Response<AwardAuditHistoryResponse>()
                {
                    Status = Status.OK,
                    Value = new AwardAuditHistoryResponse(result)
                }));
        }

        [OpenApiOperation(operationId: "CustormerProfileHistoryDocuments", tags: new[] { "Audit Microservice - CustormerProfileHistoryDocuments" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerProfileAuditHistoryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerProfileHistoryDocumentsByCustomerId")]
        public async Task<HttpResponseData> FetchCustomerProfileHistoryDocumentsByCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerId}/customerProfileAuditHistory")] HttpRequestData request, string customerId)
        {
            var result = await _mediator.Send(new CustomerProfileAuditHistoryQuery(customerId), cancellationToken: CancellationToken.None);
            return await ResultExtention<CustomerProfileAuditHistoryResponse>.ExecuteResultAsync(request,
                Result.Success(new Response<CustomerProfileAuditHistoryResponse>()
                {
                    Status = Status.OK,
                    Value = new CustomerProfileAuditHistoryResponse(result)
                }));
        }

        [OpenApiOperation(operationId: "SupplementarySilverAccountHistoryDocuments", tags: new[] { "Audit Microservice - SupplementarySilverAccountHistoryDocuments" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SupplementarySilverAuditHistoryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("SupplementarySilverHistoryDocumentsByCustomerId")]
        public async Task<HttpResponseData> FetchSupplementarySilverHistoryDocumentsByCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerId}/SupplementarySilverAuditHistory")] HttpRequestData request, string customerId)
        {
            var result = await _mediator.Send(new SupplementarySilverAuditHistoryQuery(customerId), cancellationToken: CancellationToken.None);
            return await ResultExtention<SupplementarySilverAuditHistoryResponse>.ExecuteResultAsync(request,
                Result.Success(new Response<SupplementarySilverAuditHistoryResponse>()
                {
                    Status = Status.OK,
                    Value = new SupplementarySilverAuditHistoryResponse(result)
                }));
        }

        [OpenApiOperation(operationId: "AmexCenturionHistoryDocuments", tags: new[] { "Audit Microservice - AmexCenturionHistoryDocuments" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AmexCenturionAuditHistoryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("AmexCenturionHistDocsByCustId")]
        public async Task<HttpResponseData> FetchAmexCenturionHistDocsByCustIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerId}/AmexCenturionAuditHistory")] HttpRequestData request, string customerId)
        {
            var result = await _mediator.Send(new AmexCenturionAuditHistoryQuery(customerId), cancellationToken: CancellationToken.None);
            return await ResultExtention<AmexCenturionAuditHistoryResponse>.ExecuteResultAsync(request,
                Result.Success(new Response<AmexCenturionAuditHistoryResponse>()
                {
                    Status = Status.OK,
                    Value = new AmexCenturionAuditHistoryResponse(result)
                }));
        }
    }
}