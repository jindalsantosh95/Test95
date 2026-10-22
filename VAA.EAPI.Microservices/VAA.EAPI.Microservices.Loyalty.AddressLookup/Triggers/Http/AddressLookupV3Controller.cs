using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Triggers.Http
{
    public sealed class AddressLookupV3Controller
    {
        private readonly IAddressLookupV3Service addressLookupV3Service;

        public AddressLookupV3Controller(IAddressLookupV3Service addressLookupV3Service)
        {
            this.addressLookupV3Service = addressLookupV3Service ?? throw new ArgumentNullException(nameof(addressLookupV3Service));
        }

        [Function("AddressSuggestionsV3")]
        [OpenApiOperation(operationId: "AddressSuggestionsV3", tags: new[] { "Address-Lookup Microservice" }, Summary = "Returns Find-only address suggestions", Description = "Expands nested containers with at most five concurrent Find calls and twenty expansion calls per request, returning unique Address candidates with opaque references. Access follows the existing v2 OAuth path.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AddressLookupV3SuggestionRequest), Required = true, Description = "An initial address query and country or a container reference.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddressLookupV3SuggestionResponse), Summary = "Address suggestions", Description = "Unique Address candidates after bounded recursive container expansion; no Retrieve calls.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Invalid request", Description = "The request is malformed or does not satisfy the minimum query length.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Unauthorized", Description = "The caller could not be authenticated.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Forbidden", Description = "The caller is not authorised.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Invalid reference or search too broad", Description = "The reference is invalid, expired or of the wrong type, or SearchTooBroad requires a narrower query because more than twenty container expansion calls are needed.")]
        [OpenApiResponseWithBody(statusCode: (HttpStatusCode)429, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Rate limited", Description = "The APIM rate limit was exceeded.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.ServiceUnavailable, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Unavailable", Description = "The provider or required secure configuration is unavailable.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.RequestTimeout, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Cancelled", Description = "The request was cancelled.")]
        public async Task<HttpResponseData> AddressSuggestionsV3([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v3/address-suggestions")] HttpRequestData request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync<AddressLookupV3SuggestionRequest, AddressLookupV3SuggestionResponse>(
                request,
                "InvalidSuggestionRequest",
                "An address query or container reference is required.",
                "The request was cancelled.",
                body => addressLookupV3Service.GetSuggestionsAsync(body, cancellationToken),
                cancellationToken);
        }

        [Function("AddressSelectionsV3")]
        [OpenApiOperation(operationId: "AddressSelectionsV3", tags: new[] { "Address-Lookup Microservice" }, Summary = "Retrieves one selected address", Description = "Performs one GBG Loqate Retrieve after a validated address reference is selected.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AddressLookupV3SelectionRequest), Required = true, Description = "A selected opaque address reference.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddressLookupV3SelectionResponse), Summary = "Selected address", Description = "The one formatted address returned by GBG Loqate.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Invalid request", Description = "The request is malformed.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Unauthorized", Description = "The caller could not be authenticated.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Forbidden", Description = "The caller is not authorised.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Invalid reference", Description = "The opaque reference is invalid, expired, or has the wrong candidate type.")]
        [OpenApiResponseWithBody(statusCode: (HttpStatusCode)429, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Rate limited", Description = "The APIM rate limit was exceeded.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.ServiceUnavailable, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Unavailable", Description = "The provider or required secure configuration is unavailable.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.RequestTimeout, contentType: "application/json", bodyType: typeof(AddressLookupV3ErrorResponse), Summary = "Cancelled", Description = "The request was cancelled after provider dispatch may have begun. Do not automatically retry the selection.")]
        public async Task<HttpResponseData> AddressSelectionsV3([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v3/address-selections")] HttpRequestData request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync<AddressLookupV3SelectionRequest, AddressLookupV3SelectionResponse>(
                request,
                "InvalidSelectionRequest",
                "An address reference is required.",
                "The request was cancelled after provider dispatch may have begun.",
                body => addressLookupV3Service.GetSelectionAsync(body, cancellationToken),
                cancellationToken);
        }

        private async Task<HttpResponseData> ExecuteAsync<TRequest, TResponse>(
            HttpRequestData request,
            string invalidRequestCode,
            string invalidRequestMessage,
            string cancellationMessage,
            Func<TRequest, Task<TResponse>> operation,
            CancellationToken cancellationToken)
            where TRequest : class
        {
            try
            {
                var body = await DeserializeRequestAsync<TRequest>(request, invalidRequestCode, invalidRequestMessage, cancellationToken);
                var response = await operation(body);

                return await CreateResponseAsync(request, HttpStatusCode.OK, response, cancellationToken);
            }
            catch (AddressLookupV3Exception exception)
            {
                return await CreateResponseAsync(request, exception.StatusCode, new AddressLookupV3ErrorResponse { Code = exception.Code, Message = exception.Message }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return await CreateResponseAsync(request, HttpStatusCode.RequestTimeout, new AddressLookupV3ErrorResponse { Code = "Cancelled", Message = cancellationMessage }, CancellationToken.None);
            }
            catch
            {
                return await CreateResponseAsync(request, HttpStatusCode.ServiceUnavailable, new AddressLookupV3ErrorResponse { Code = "Unavailable", Message = "Address lookup is temporarily unavailable." }, CancellationToken.None);
            }
        }

        private static async Task<T> DeserializeRequestAsync<T>(HttpRequestData request, string errorCode, string errorMessage, CancellationToken cancellationToken)
            where T : class
        {
            try
            {
                var body = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);
                if (body.IsNullOrWhiteSpace())
                    throw new AddressLookupV3Exception(HttpStatusCode.BadRequest, errorCode, errorMessage);

                return Json.Deserialize<T>(body)
                    ?? throw new AddressLookupV3Exception(HttpStatusCode.BadRequest, errorCode, errorMessage);
            }
            catch (JsonException)
            {
                throw new AddressLookupV3Exception(HttpStatusCode.BadRequest, errorCode, errorMessage);
            }
            catch (ArgumentException)
            {
                throw new AddressLookupV3Exception(HttpStatusCode.BadRequest, errorCode, errorMessage);
            }
        }

        private static async Task<HttpResponseData> CreateResponseAsync<T>(HttpRequestData request, HttpStatusCode statusCode, T body, CancellationToken cancellationToken)
        {
            var response = request.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(body, cancellationToken);

            return response;
        }
    }
}
