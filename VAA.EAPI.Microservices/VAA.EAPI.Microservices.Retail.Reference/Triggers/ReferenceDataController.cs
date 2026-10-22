using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Reference.Application;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Trips;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Model.Air4;

namespace VAA.EAPI.Microservices.Retail.Reference.Triggers
{
    public class ReferenceDataController
    {
        private readonly IMediator _mediator;
        private readonly ReferenceDataCommandHandler _referenceDataCommandHandler;

        public ReferenceDataController(IMediator mediator, ReferenceDataCommandHandler referenceDataCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _referenceDataCommandHandler = referenceDataCommandHandler ?? throw new ArgumentNullException(nameof(referenceDataCommandHandler));
        }

        [OpenApiOperation(operationId: "TaxCode", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for taxCodes", Description = "Endpoint to fetch reference data of TaxCode", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<CodeValuePair>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("TaxCode")]
        public async Task<HttpResponseData> TaxCode([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TaxCode")] HttpRequestData req)
        {
            var taxcodeDetailsResponse = Result.Success(new Response<IEnumerable<CodeValuePair>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<TaxCode>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.TAXCODE
                }))?.Values
            });
            return await ResultExtention<IEnumerable<CodeValuePair>>.ExecuteResultAsync(req, taxcodeDetailsResponse);
        }

        [OpenApiOperation(operationId: "VsEcon", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for VsEcon", Description = "Endpoint to fetch reference data of VsEcon", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(VsEcon), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("VsEcon")]
        public async Task<HttpResponseData> VsEcon([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "VsEcon")] HttpRequestData req)
        {
            return await ResultExtention<VsEcon>.ExecuteResultAsync(req, Result.Success(new Response<VsEcon>
            {
                Status = Status.OK,
                Value = Json.Deserialize<VsEcon>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.VSECON
                })),
            }));
        }

        [OpenApiOperation(operationId: "ProductIdentifiers", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for product identifiers", Description = "Endpoint to fetch reference data of ProductIdentifiers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<ProductValue>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ProductIdentifiers")]
        public async Task<HttpResponseData> ProductIdentifies([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ProductIdentifiers")] HttpRequestData req)
        {
            var productIdentifiersResponse = Result.Success(new Response<IEnumerable<ProductValue>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<ProductIndentifiers>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.PRODUCTIDENTIFIRES
                }))?.Values,
            });

            return await ResultExtention<IEnumerable<ProductValue>>.ExecuteResultAsync(req, productIdentifiersResponse);
        }

        [OpenApiOperation(operationId: "AirportData", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for airport", Description = "Endpoint to fetch reference data of AirportDetail", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<AirportDetail>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("AirportData")]
        public async Task<HttpResponseData> AirportData([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AirportDetail")] HttpRequestData req)
        {
            var airportData = Json.Deserialize<AirportReferenceData>(await _mediator.Send(new ReferenceDataCommand
            {
                ReferenceDataType = ReferenceDataTypeEnum.AIRPORT

            }))?.AirportDetails;

            var airportCodes = (req.Query["airportCodes"]?.Split(',') ?? Array.Empty<string>()).Where(x => !x.IsNullOrWhiteSpace()).ToArray();

            if (airportData != null && airportCodes.Any())
                airportData = airportData.Where(x => airportCodes.Any(c => c.Is(x.AirportCode)));

            return await ResultExtention<IEnumerable<AirportDetail>>.ExecuteResultAsync(req, Result.Success(new Response<IEnumerable<AirportDetail>> { Status = Status.OK, Value = airportData }));
        }

        [OpenApiOperation(operationId: "AirlineDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for airport", Description = "Endpoint to fetch reference data of airlines", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<AirlineDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("AirlineDetails")]
        public async Task<HttpResponseData> AirlineDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AirlineDetails")] HttpRequestData req)
        {

            var airlineDetailsResponse = Result.Success(new Response<IEnumerable<AirlineDetails>>

            {
                Status = Status.OK,
                Value = Json.Deserialize<AirlineReferenceData>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.AIRLINE

                }))?.AirlineDetails
            });

            return await ResultExtention<IEnumerable<AirlineDetails>>.ExecuteResultAsync(req, airlineDetailsResponse);
        }

        [OpenApiOperation(operationId: "CabinDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for Cabin", Description = "Endpoint to fetch reference data of Cabin", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<CabinDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CabinDetails")]
        public async Task<HttpResponseData> CabinDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CabinDetails")] HttpRequestData req)
        {
            var cabinDetailsResponse = Result.Success(new Response<IEnumerable<CabinDetails>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<CabinReferenceData>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.CABIN

                }))?.CabinDetails
            });

            return await ResultExtention<IEnumerable<CabinDetails>>.ExecuteResultAsync(req, cabinDetailsResponse);
        }

        [OpenApiOperation(operationId: "AircraftDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for aircrafts", Description = "Endpoint to fetch reference data of aircrafts", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<AircraftDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("AircraftDetails")]
        public async Task<HttpResponseData> AircraftDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AircraftDetails")] HttpRequestData req)
        {

            var aircraftDetailsResponse = Result.Success(new Response<IEnumerable<AircraftDetails>>

            {
                Status = Status.OK,
                Value = Json.Deserialize<AircraftReferenceData>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.AIRCRAFT

                }))?.AircraftDetails
            });

            return await ResultExtention<IEnumerable<AircraftDetails>>.ExecuteResultAsync(req, aircraftDetailsResponse);
        }

        [OpenApiOperation(operationId: "BaggageDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for BaggageDetails", Description = "Endpoint to fetch reference data of baggage details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<BaggageDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("BaggageDetails")]
        public async Task<HttpResponseData> BaggageDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BaggageDetails")] HttpRequestData req)
        {

            var baggageDetails = Result.Success(new Response<IEnumerable<BaggageDetails>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<IEnumerable<BaggageDetails>>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.BAGGAGE

                }))
            });

            return await ResultExtention<IEnumerable<BaggageDetails>>.ExecuteResultAsync(req, baggageDetails);
        }

        [OpenApiOperation(operationId: "Countries", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for Countries", Description = "Endpoint to fetch reference data of Countries", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<Country>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Countries")]
        public async Task<HttpResponseData> Countries([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Countries")] HttpRequestData req)
        {
            var countriesResponse = Result.Success(new Response<IEnumerable<Country>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<List<Country>>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.COUNTRY
                }))
            });

            return await ResultExtention<IEnumerable<Country>>.ExecuteResultAsync(req, countriesResponse);
        }

        [OpenApiOperation(operationId: "UpgradeOfferDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for Offers", Description = "Endpoint to fetch reference data of Upgrade Offer Details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<UpgradeOfferDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpgradeOfferDetails")]
        public async Task<HttpResponseData> UpgradeOfferDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UpgradeOfferDetails")] HttpRequestData req)
        {
            var upgradeOfferDetailsDataResponse = Result.Success(new Response<IEnumerable<UpgradeOfferDetails>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<UpgradeOfferDetailsReferenceData>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.Offers
                }))?.Upsell
            });

            return await ResultExtention<IEnumerable<UpgradeOfferDetails>>.ExecuteResultAsync(req, upgradeOfferDetailsDataResponse);
        }

        [OpenApiOperation(operationId: "Air4CurrencyNumberDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for Air4 currency number", Description = "Endpoint to fetch reference data of Air4 currency number", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<Air4CurrencyNumberDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Air4CurrencyNumberDetails")]
        public async Task<HttpResponseData> Air4CurrencyNumberDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Air4CurrencyNumberDetails")] HttpRequestData req)
        {
            var currencyNumberDetails = Result.Success(new Response<IEnumerable<Air4CurrencyNumberDetails>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<List<Air4CurrencyNumberDetails>>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.AIR4CURRENCYNUMBER
                }))
            });

            return await ResultExtention<IEnumerable<Air4CurrencyNumberDetails>>.ExecuteResultAsync(req, currencyNumberDetails);
        }

        [OpenApiOperation(operationId: "SSRDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for SSR Details", Description = "Endpoint to fetch reference data of SSR Details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<SSRDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SSRDetails")]
        public async Task<HttpResponseData> SSRDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SSRDetails")] HttpRequestData req)
        {
            var ssrDetails = Result.Success(new Response<IEnumerable<SSRDetails>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<SSRDetailsReferenceData>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.SSRDETAIL

                }))?.SSRDetails
            });

            return await ResultExtention<IEnumerable<SSRDetails>>.ExecuteResultAsync(req, ssrDetails);
        }


        [OpenApiOperation(operationId: "FQTPrograms", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for FQT Programs", Description = "Endpoint to fetch reference data of SSR Details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<FQTProgram>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FQTPrograms")]
        public async Task<HttpResponseData> FQTPrograms([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FQTPrograms")] HttpRequestData req)
        {
            var fqtDetails = Json.Deserialize<IEnumerable<FQTProgram>>(await _mediator.Send(new ReferenceDataCommand() { ReferenceDataType = ReferenceDataTypeEnum.FQTPROGS }));

            var response = Result.Success(new Response<IEnumerable<FQTProgram>>
            {
                Status = Status.OK,
                Value = fqtDetails.OrderBy(f => f.AirlineName)
            });

            return await ResultExtention<IEnumerable<FQTProgram>>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "CabinClass", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for Cabin Class brand mapping", Description = "Endpoint to fetch reference data of Cabin Class brand mapping", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<CabinClassDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CabinClass")]
        public async Task<HttpResponseData> CabinClass([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CabinClass")] HttpRequestData req)
        {
            var cabinClassDetails = Json.Deserialize<IEnumerable<CabinClassDetails>>(await _mediator.Send(new ReferenceDataCommand() { ReferenceDataType = ReferenceDataTypeEnum.CABINCLASS }));

            var response = Result.Success(new Response<IEnumerable<CabinClassDetails>>
            {
                Status = Status.OK,
                Value = cabinClassDetails.OrderBy(f => f.CabinClass)
            });

            return await ResultExtention<IEnumerable<CabinClassDetails>>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "SSRMealDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for SSR and Meal Details", Description = "Endpoint to fetch reference data of SSR and Meal Details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<SSRMealDetail>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("SSRMealDetails")]
        public async Task<HttpResponseData> SSRMealDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SSRMealDetails")] HttpRequestData req)
        {
            var ssrMealDetails = Result.Success(new Response<IEnumerable<SSRMealDetail>>
            {
                Status = Status.OK,
                Value = Json.Deserialize<SSRMealDetailsReferenceData>(await _mediator.Send(new ReferenceDataCommand
                {
                    ReferenceDataType = ReferenceDataTypeEnum.SSRMEALDETAILS

                }))?.SSRMealDetails
            });

            return await ResultExtention<IEnumerable<SSRMealDetail>>.ExecuteResultAsync(req, ssrMealDetails);
        }

        [OpenApiOperation(operationId: "PaymentMethodDetails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch payment method details for vhols bookings", Description = "Fetch payment method details for vhols bookings", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<PaymentMethod>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("PaymentMethodDetails")]
        public async Task<HttpResponseData> PaymentMethodDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PaymentMethodDetails")] HttpRequestData req)
        {
            var paymentMethodDetails = Json.Deserialize<PaymentMethodReferenceData>(await _referenceDataCommandHandler.Handle(new ReferenceDataCommand
            {
                ReferenceDataType = ReferenceDataTypeEnum.PAYMENTMETHODDETAILS

            }, req.FunctionContext.CancellationToken))?.PaymentMethodDetails;

            return await ResultExtention<IEnumerable<PaymentMethod>>.ExecuteResultAsync(req, Result.Success(new Response<IEnumerable<PaymentMethod>> { Status = Status.OK, Value = paymentMethodDetails }));
        }
    }
}
