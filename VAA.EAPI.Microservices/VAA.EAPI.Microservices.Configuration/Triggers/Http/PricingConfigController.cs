using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Configuration.Application;
using VAA.EAPI.Microservices.Configuration.Application.QueriesAndHandlers;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Triggers.Http
{
    public class PricingConfigController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PricingConfigController> _logger;

        public PricingConfigController(IMediator mediator, ILogger<PricingConfigController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "Currency Lookup", tags: new[] { "Currency Lookup" }, Summary = "This endpoint search the currency associated with particular agency", Description = "This endpoint search currency associated with particular agency", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "countryCode", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Country Code", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(CurrencyData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]

        [Function("PosCurrency")]
        public async Task<HttpResponseData> PosCurrency([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PosCurrency/{countryCode}")] HttpRequestData req, string countryCode)
        {
            var commandOutput = await _mediator.Send(new CurrencyDBQuery { CountryCode = countryCode }, CancellationToken.None);
            return await ResultExtention<CurrencyData>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "UpsellOffers Lookup", tags: new[] { "UpsellOffers Lookup" }, Summary = "This endpoint fetch upsell offers", Description = "This endpoint fetch upsell offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(UpsellOffersData), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpsellOffers")]
        public async Task<HttpResponseData> UpsellOffers([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UpsellOffers")] HttpRequestData req)
        {
            try
            {
                var commandOutput = await _mediator.Send(new UpsellOffersDBQuery(), CancellationToken.None);

                var result = CSharpFunctionalExtensions.Result.Success(new Response<IEnumerable<UpsellOffersData>>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<IEnumerable<UpsellOffersData>>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching Upsell Offers");

                var result = CSharpFunctionalExtensions.Result.Failure<Response<IEnumerable<UpsellOffersData>>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<IEnumerable<UpsellOffersData>>.ExecuteResultAsync(req, result);
            }
        }

        [OpenApiOperation(operationId: "PricingLocation Lookup", tags: new[] { "PricingLocation Lookup" }, Summary = "This endpoint search the CountryCode associated with particular PricingLocation", Description = "This endpoint search the CountryCode associated with particular PricingLocation", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(PricingLocation), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]

        [Function("PricingLocation")]
        public async Task<HttpResponseData> PricingLocation([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PricingLocation")] HttpRequestData req)
        {
            try
            {
                var commandOutput = await _mediator.Send(new PricingLocationDbQuery(), CancellationToken.None);

                return await ResultExtention<PricingLocation>.ExecuteResultAsync(req, commandOutput);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching Pricing Location");

                var result = CSharpFunctionalExtensions.Result.Failure<Response<PricingLocation>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<PricingLocation>.ExecuteResultAsync(req, result);
            }
        }

        [OpenApiOperation(operationId: "FareFamily Lookup", tags: new[] { "FareFamily Lookup" }, Summary = "This endpoint for retrieving the fare family list", Description = "This endpoint for retrieving the fare family list", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FareFamilyList), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]

        [Function("FareFamily")]
        public async Task<HttpResponseData> FareFamily([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FareFamily")] HttpRequestData req)
        {
            try
            {
                var commandOutput = await _mediator.Send(new FareFamilyQuery(), CancellationToken.None);

                var result = CSharpFunctionalExtensions.Result.Success(new Response<IEnumerable<FareFamilyList>>
                {
                    Status = Status.OK,
                    Value = commandOutput
                });

                return await ResultExtention<IEnumerable<FareFamilyList>>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching FareFamily");

                var result = CSharpFunctionalExtensions.Result.Failure<Response<FareFamilyList>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<FareFamilyList>.ExecuteResultAsync(req, result);
            }
        }
    }
}
