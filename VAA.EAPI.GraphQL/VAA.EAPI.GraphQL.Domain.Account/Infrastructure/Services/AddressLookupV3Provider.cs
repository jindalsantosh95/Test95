using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services
{
    public sealed class AddressLookupV3Provider : IAddressLookupV3Provider
    {
        private readonly IHttpConnector httpConnector;
        private readonly IOAuthTokenProvider oauthTokenProvider;

        public AddressLookupV3Provider(IHttpConnector httpConnector, IOAuthTokenProvider oauthTokenProvider)
        {
            this.httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            this.oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
        }

        public async Task<AddressLookupV3SuggestionResponse> GetSuggestionsAsync(AddressLookupV3SuggestionInput request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            var query = request.Query;
            if (!request.PostCode.IsNullOrWhiteSpace() || !request.AddressLine1.IsNullOrWhiteSpace())
            {
                if (!query.IsNullOrWhiteSpace() || !request.Reference.IsNullOrWhiteSpace())
                    throw CreateError("Address fields cannot be combined with a query or container reference.", 400);

                query = string.Join(" ", new[] { request.AddressLine1?.Trim(), request.PostCode?.Trim() }
                    .Where(value => !value.IsNullOrWhiteSpace()));
            }

            return await SendAsync<AddressLookupV3SuggestionResponse>("v3/address-suggestions",
                new { Query = query, request.CountryCode, request.Reference }, cancellationToken);
        }

        public async Task<AddressLookupV3SelectionResponse> GetSelectionAsync(AddressLookupV3SelectionInput request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            return await SendAsync<AddressLookupV3SelectionResponse>("v3/address-selections", request, cancellationToken);
        }

        private async Task<T> SendAsync<T>(string relativePath, object requestBody, CancellationToken cancellationToken)
            where T : class
        {
            var endpoint = GetEndpoint(relativePath);
            var subscriptionKey = ConfigReader.Try(Common.Constants.Environment.EnrolmentMSOcpKey);
            if (subscriptionKey.IsNullOrWhiteSpace())
                throw CreateError("Address Lookup v3 is temporarily unavailable.", 503);

            // Reuse v2's application identity, not an enrolling user's token.
            var bearerToken = await oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AddressLookupAPIAppId));
            if (bearerToken.IsNullOrWhiteSpace())
                throw CreateError("Address Lookup v3 is temporarily unavailable.", 503);
            cancellationToken.ThrowIfCancellationRequested();

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(Json.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation(Common.Constants.Application.OcpKeyHeader, subscriptionKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            // The existing non-generic overload adds x-nowait once, avoiding duplicate values.
            using var response = await httpConnector.SendAsync(request, cancellationToken, noWait: true);
            var result = Json.Deserialize<T>(await response.Content.ReadAsStringAsync(cancellationToken));
            if (result is null)
                throw new InvalidOperationException("Address Lookup v3 returned an invalid response.");

            return result;
        }

        private static GraphQLException CreateError(string message, int status) =>
            new(ErrorBuilder.New().SetMessage(message).SetCode(status.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .SetExtension("httpStatusCode", status)
                .SetExtension("transportStatusCode", status)
                .Build());

        private static Uri GetEndpoint(string relativePath)
        {
            var configuredEndpoint = ConfigReader.Get(Constants.AddressLookupV3Settings.AddressLookupV3ApimUrl);
            if (configuredEndpoint.IsNullOrWhiteSpace() || !Uri.TryCreate(configuredEndpoint, UriKind.Absolute, out var baseUri))
                throw new InvalidOperationException("Address Lookup v3 is not configured.");

            var isLocalDevelopment = ConfigReader.Try("AZURE_FUNCTIONS_ENVIRONMENT").Is("Development") &&
                ConfigReader.Try("WEBSITE_INSTANCE_ID").IsNullOrWhiteSpace();
            var isAllowedLocalTarget = isLocalDevelopment && baseUri.IsLoopback &&
                (baseUri.Scheme.Is(Uri.UriSchemeHttp) || baseUri.Scheme.Is(Uri.UriSchemeHttps));
            if ((baseUri.IsLoopback && !isAllowedLocalTarget) || (!baseUri.Scheme.Is(Uri.UriSchemeHttps) && !isAllowedLocalTarget))
                throw new InvalidOperationException("Address Lookup v3 is not configured.");

            var builder = new UriBuilder(baseUri);
            builder.Path = builder.Path.TrimEnd('/') + "/" + relativePath;
            builder.Query = string.Empty;

            return builder.Uri;
        }

    }
}
