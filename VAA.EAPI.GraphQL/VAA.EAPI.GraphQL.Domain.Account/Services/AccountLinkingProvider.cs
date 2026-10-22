using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public sealed class AccountLinkingProvider : IAccountLinkingProvider
    {
        private readonly IApplicationOAuthTokenProvider _tokens;
        private readonly IHttpConnector _http;
        public AccountLinkingProvider(IApplicationOAuthTokenProvider tokens, IHttpConnector http)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public Task<CreateLinkingProofResponse> CreateAsync(CreateLinkingProofRequest request) =>
            SendAsync<CreateLinkingProofResponse>(Constants.Application.AccountLinkingProofEndpoint, request, "The account-linking proof request is invalid.");

        public Task<RedeemLinkingProofResponse> RedeemAsync(RedeemLinkingProofRequest request) =>
            SendAsync<RedeemLinkingProofResponse>(Constants.Application.AccountLinkingProofRedemptionEndpoint, request, "The account-linking proof is invalid, expired, or does not match the transaction.");

        private async Task<T> SendAsync<T>(string endpoint, object request, string validationMessage)
        {
            try
            {
                var headers = new Dictionary<string, string> { [Application.OcpKeyHeader] = ConfigReader.Get(Environment.IdentityOcpKey) };
                var token = await _tokens.AcquireApplicationTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId));
                var response = await _http.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{endpoint}", headers, token, HttpMethod.Post, Json.Serialize(request));
                return response.Response;
            }
            catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw AccountLinkingError.Create(
                    endpoint == Constants.Application.AccountLinkingProofEndpoint ? "ACCOUNT_LINKING_TRANSACTION_INVALID" : "ACCOUNT_LINKING_PROOF_INVALID",
                    validationMessage, 400);
            }
            catch (HttpRequestException exception) when (exception.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            {
                throw AccountLinkingError.Create("ACCOUNT_LINKING_CLIENT_FORBIDDEN", "The caller is not authorized to use account linking.", 403);
            }
            catch (Exception exception) when (exception is HttpRequestException or TimeoutException or TaskCanceledException)
            {
                throw AccountLinkingError.Create("ACCOUNT_LINKING_DEPENDENCY_UNAVAILABLE", "Account linking is temporarily unavailable.", 503, true);
            }
        }
    }
}
