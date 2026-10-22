using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command.GbgService;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Query;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Triggers.Http
{
    public class AddressDetailsController
    {
        private readonly IMediator _mediator;
        private readonly IAddressLookupService _addressLookupService;

        public AddressDetailsController(IMediator mediator, IAddressLookupService addressLookupSevice)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _addressLookupService = addressLookupSevice ?? throw new ArgumentNullException(nameof(addressLookupSevice));
        }

        [Function("AddressDetails")]
        [OpenApiOperation(operationId: "AddressDetails", tags: new[] { "Address-Lookup Microservice" }, Summary = "Provides Address details by Postcode and streen Name", Description = "This should contain the Address Details by connecting with Address Lookup QAS Endpoint.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(AddressDetailsRequest), Required = true, Description = "This should contain the Request Information to Search for the Address Lookup.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<AddressDetail>), Summary = "Address Details from QAS", Description = "Address Details by Streename and Postcode from QAS service")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> AddressDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddressDetails")] HttpRequestData req)
        {
            var addressDetailsRequest = Json.Deserialize<AddressDetailsRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            return await ResultExtention<List<AddressDetail>>.ExecuteResultAsync(req, await _mediator.Send(new AddressLookupQuery(addressDetailsRequest.CountryCode, addressDetailsRequest.PostCode, addressDetailsRequest.AddressLine1), CancellationToken.None));
        }

        [Function("AddressDetailsV2")]
        [OpenApiOperation(operationId: "AddressDetailsV2", tags: new[] { "Address-Lookup Microservice" }, Summary = "Provides Address details by Postcode and streen Name", Description = "This should contain the Address Details by connecting with Address Lookup Azure Endpoint.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(AddressDetailsRequest), Required = true, Description = "This should contain the Request Information to Search for the Address Lookup Azure Endpoint.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<AddressDetail>), Summary = "Address Details by Postcode", Description = "Address Details by Streename and Postcode from Cloud QAS Service")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> AddressDetailsV2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddressDetailsV2")] HttpRequestData req)
        {
            var addressDetailsRequest = Json.Deserialize<AddressDetailsRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            return await ResultExtention<IEnumerable<AddressDetail>>.ExecuteResultAsync(req, await _addressLookupService.GetAllAddresses(addressDetailsRequest.CountryCode, addressDetailsRequest.PostCode, addressDetailsRequest.AddressLine1));
        }

        [Function("FindAddress")]
        [OpenApiOperation(operationId: "FindAddress", tags: new[] { "Address-Lookup Microservice" }, Summary = "Provides Address details by address text", Description = "This should contain the Address Id Details by connecting with GBG locate service", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "addressText", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Address Text")]
        [OpenApiParameter(name: "countryCode", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "countryCode")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<AddressDetail>), Summary = "Address Details by streetname and Postcode", Description = "Address Details by Streename and Postcode from GBG locate service")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> FindAddress([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FindAddress/{addressText}/{countryCode}")] HttpRequestData req, string addressText, string countryCode)
        {
            return await ResultExtention<IEnumerable<AddressDetail>>.ExecuteResultAsync(req, await _addressLookupService.FindAddresses(addressText, countryCode));
        }

        [Function("RetrieveAddress")]
        [OpenApiOperation(operationId: "RetrieveAddress", tags: new[] { "Address-Lookup Microservice" }, Summary = "Provides Address details by address Id", Description = "This should retrieve the Address Details by connecting with GBG locate service", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "addressId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Address Id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<AddressDetail>), Summary = "Retrieve Address Details by Address ID", Description = "Retrieve Address Details by Address ID from GBG locate service")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(IEnumerable<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> RetrieveAddress([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Addresses/{addressId}")] HttpRequestData req, string addressId)
        {
            return await ResultExtention<IEnumerable<AddressDetail>>.ExecuteResultAsync(req, await _addressLookupService.RetrieveAddress(addressId));
        }
    }
}
