using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3
{
    public sealed class GbgV3Service : IGbgV3Service
    {
        private readonly IHttpConnector httpConnector;

        public GbgV3Service(IHttpConnector httpConnector)
        {
            this.httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<GbgResponse<FindResponse>> FindAsync(GbgV3FindRequest request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            var hasText = !request.Text.IsNullOrWhiteSpace();
            var hasContainer = !request.Container.IsNullOrWhiteSpace();
            if (hasText == hasContainer)
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.BadRequest, "InvalidSuggestionRequest", "Supply either an address query or an address reference.");

            var parameters = new List<KeyValuePair<string, string>>
            {
                new("Key", GetGbgServiceKey()),
                new("Countries", request.CountryCode),
                new("Limit", request.Limit.ToString(System.Globalization.CultureInfo.InvariantCulture))
            };

            parameters.Add(hasText ? new KeyValuePair<string, string>("Text", request.Text) : new KeyValuePair<string, string>("Container", request.Container));

            return await SendAsync<FindResponse>(ConfigReader.Get(Constants.Environment.GbgV3FindEndpoint), parameters, cancellationToken);
        }

        public async Task<GbgResponse<RetrieveResponse>> RetrieveAsync(string providerAddressId, CancellationToken cancellationToken)
        {
            if (providerAddressId.IsNullOrWhiteSpace())
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");

            var parameters = new List<KeyValuePair<string, string>>
            {
                new("Key", GetGbgServiceKey()),
                new("Id", providerAddressId)
            };

            return await SendAsync<RetrieveResponse>(ConfigReader.Get(Constants.Environment.GbgV3RetrieveEndpoint), parameters, cancellationToken);
        }

        private async Task<GbgResponse<T>> SendAsync<T>(string endpoint, IEnumerable<KeyValuePair<string, string>> parameters, CancellationToken cancellationToken)
        {
            var requestUri = BuildUri(endpoint, parameters);
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.SuppressRequestUriLogging();
            request.Headers.TryAddWithoutValidation("x-nowait", "true");

            using var response = await httpConnector.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ProviderUnavailable", "Address lookup is temporarily unavailable.");

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = Json.Deserialize<GbgResponse<T>>(body);
            if (result is null)
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ProviderInvalidResponse", "Address lookup is temporarily unavailable.");

            return result;
        }

        private static Uri BuildUri(string endpoint, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (endpoint.IsNullOrWhiteSpace() || !Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) || !endpointUri.Scheme.Is(Uri.UriSchemeHttps))
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ProviderConfigurationUnavailable", "Address lookup is temporarily unavailable.");

            if (!endpointUri.Query.IsNullOrWhiteSpace())
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ProviderConfigurationUnavailable", "Address lookup is temporarily unavailable.");

            var builder = new UriBuilder(endpointUri)
            {
                Query = string.Join("&", parameters.Select(parameter => $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"))
            };

            return builder.Uri;
        }

        private static string GetGbgServiceKey()
        {
            var configuredMap = ConfigReader.Try(Constants.Environment.GbgServiceKeyMap);
            if (configuredMap.IsNullOrWhiteSpace())
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ProviderKeyUnavailable", "Address lookup is temporarily unavailable.");

            IEnumerable<GBGConfig> configuredKeys;
            try
            {
                configuredKeys = Json.Deserialize<IEnumerable<GBGConfig>>(configuredMap);
            }
            catch (JsonException)
            {
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ProviderKeyUnavailable", "Address lookup is temporarily unavailable.");
            }

            // Anonymous enrolment uses the same default entry as the v2 no-token path.
            var configuredKey = configuredKeys?.FirstOrDefault(item => item is not null && item.ClientId.IsNullOrWhiteSpace());

            if (configuredKey?.GbgKey.IsNullOrWhiteSpace() ?? true)
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ProviderKeyUnavailable", "Address lookup is temporarily unavailable.");

            return configuredKey.GbgKey;
        }
    }
}
