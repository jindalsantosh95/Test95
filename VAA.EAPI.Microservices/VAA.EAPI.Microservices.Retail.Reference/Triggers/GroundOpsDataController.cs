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
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Reference.Application;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.OCI;

namespace VAA.EAPI.Microservices.Retail.Reference.Triggers
{
    public class GroundOpsDataController
    {
        private readonly IRequestHandler<ReferenceDataCommand, string> _referenceDataCommandHandler;

        public GroundOpsDataController(IRequestHandler<ReferenceDataCommand, string> referenceDataCommandHandler)
        {
            _referenceDataCommandHandler = referenceDataCommandHandler ?? throw new ArgumentNullException(nameof(referenceDataCommandHandler));
        }

        [OpenApiOperation(operationId: "Genders", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for Genders", Description = "Endpoint to fetch reference data of Genders", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<Gender>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GenderDetails")]
        public async Task<HttpResponseData> GenderDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GenderDetails")] HttpRequestData req)
        {
            var genderResponse = Result.Success(new Response<IEnumerable<Gender>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<IEnumerable<Gender>>(await _referenceDataCommandHandler.Handle(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.GENDERS
                }, CancellationToken.None))
            });

            return await ResultExtention<IEnumerable<Gender>>.ExecuteResultAsync(req, genderResponse);
        }

        [OpenApiOperation(operationId: "StatesOfUSA", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for US States", Description = "Endpoint to fetch reference data of Genders", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<StatesOfUSA>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("StatesOfUSADetails")]
        public async Task<HttpResponseData> StatesOfUSADetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "StatesOfUSADetails")] HttpRequestData req)
        {
            var statesResponse = Result.Success(new Response<IEnumerable<StatesOfUSA>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<IEnumerable<StatesOfUSA>>(await _referenceDataCommandHandler.Handle(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.STATESOFUSA
                }, CancellationToken.None))
            });

            return await ResultExtention<IEnumerable<StatesOfUSA>>.ExecuteResultAsync(req, statesResponse);
        }

        [OpenApiOperation(operationId: "Nationalities", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for Nationalities", Description = "Endpoint to fetch reference data of Nations", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<Nationality>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("NationalityDetails")]
        public async Task<HttpResponseData> NationalityDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "NationalityDetails")] HttpRequestData req)
        {
            var nationalityResponse = Result.Success(new Response<IEnumerable<Nationality>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<IEnumerable<Nationality>>(await _referenceDataCommandHandler.Handle(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.NATIONALITIES
                }, CancellationToken.None))
            });

            return await ResultExtention<IEnumerable<Nationality>>.ExecuteResultAsync(req, nationalityResponse);
        }


        [OpenApiOperation(operationId: "PassportCountries", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for Nationalities", Description = "Endpoint to fetch reference data of Nations", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<PassportCountry>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("PassportCountryDetails")]
        public async Task<HttpResponseData> PassportCountryDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PassportCountryDetails")] HttpRequestData req)
        {
            var passportCountryResponse = Result.Success(new Response<IEnumerable<PassportCountry>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<IEnumerable<PassportCountry>>(await _referenceDataCommandHandler.Handle(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.PASSPORTCOUNTRIES
                }, CancellationToken.None))
            });

            return await ResultExtention<IEnumerable<PassportCountry>>.ExecuteResultAsync(req, passportCountryResponse);
        }
    }
}
