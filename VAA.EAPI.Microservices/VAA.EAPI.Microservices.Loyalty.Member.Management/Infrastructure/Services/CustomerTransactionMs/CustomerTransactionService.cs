using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs
{
    public class CustomerTransactionService : ICustomerTransactionService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _oauthTokenProvider;

        public CustomerTransactionService(IHttpConnector httpConnector, IOAuthTokenProvider oauthTokenProvider)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
        }

        public Task<TransactionCommandStatusResponse> GetTransactionStatusAsync(string requestId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
            return ExecuteAsync<TransactionCommandStatusResponse>($"{Constants.Application.CustomerTransactionStatusEndpoint}/{Uri.EscapeDataString(requestId)}", HttpMethod.Get, cancellationToken);
        }

        public Task<TResponse> ExecuteAsync<TResponse>(string operation, HttpMethod method, CancellationToken cancellationToken)
        {
            return ExecuteAsync<object, TResponse>(operation, method, null, cancellationToken);
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(string operation, HttpMethod method, TRequest request, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(operation);
            ArgumentNullException.ThrowIfNull(method);

            var headers = new Dictionary<string, string> { [Constants.Environment.OcpKeyHeader] = ConfigReader.Get(Constants.Environment.CustomerTransactionMSSubscriptionKey) };
            var token = await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.CustomerTxnMSResourceId));
            var uri = BuildOperationUri(operation);

            using var content = request is null ? null : new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json");
            using var response = await _httpConnector.SendAsync(uri, headers, token, method, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return Json.Deserialize<TResponse>(responseContent) ?? throw new InvalidOperationException("Customer Transactions returned an empty response.");
        }

        private static string BuildOperationUri(string operation)
        {
            var baseUri = ConfigReader.Get(Constants.Environment.CustomerTxnMSUrl);

            if (!Uri.TryCreate(baseUri, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException("Customer Transactions base URL configuration is invalid.");
            }

            if (Uri.TryCreate(operation, UriKind.Absolute, out _) || operation.StartsWith("//", StringComparison.Ordinal))
            {
                throw new ArgumentException("Customer Transactions operation must be a relative path.", nameof(operation));
            }

            return $"{baseUri.TrimEnd('/')}/{operation.TrimStart('/')}";
        }
    }
}
