using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs
{
    public class RulesEngineService : IRulesEngineService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _oauthTokenProvider;

        public RulesEngineService(IHttpConnector httpConnector, IOAuthTokenProvider oauthTokenProvider)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
        }

        public async Task<EarnAirlineAccrualCalculatorResponse> EarnAirlineAccrualCalculatorAsync(EarnAirlineAccrualCalculatorRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await ExecuteAsync<EarnAirlineAccrualCalculatorRequest, EarnAirlineAccrualCalculatorResponse>(
                Constants.Application.RuleEngineAirlinePointsEarnCalculatorEndpoint,
                HttpMethod.Post,
                request,
                cancellationToken);
        }

        public Task<TResponse> ExecuteAsync<TResponse>(string operation, HttpMethod method, CancellationToken cancellationToken)
        {
            return ExecuteAsync<object, TResponse>(operation, method, null, cancellationToken);
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(string operation, HttpMethod method, TRequest request, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(operation);
            ArgumentNullException.ThrowIfNull(method);

            var headers = new Dictionary<string, string> { [Constants.Environment.OcpKeyHeader] = ConfigReader.Get(Constants.Environment.RuleEngineOcpSubscriptionId) };
            var token = await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.RuleEngineAppId));
            var uri = BuildOperationUri(operation);

            using var content = request is null ? null : new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json");
            using var response = await _httpConnector.SendAsync(uri, headers, token, method, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return Json.Deserialize<TResponse>(responseContent) ?? throw new InvalidOperationException("Rules Engine returned an empty response.");
        }

        private static string BuildOperationUri(string operation)
        {
            var baseUri = ConfigReader.Get(Constants.Environment.RuleEngineUrl);

            if (!Uri.TryCreate(baseUri, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException("Rules Engine base URL configuration is invalid.");
            }

            if (Uri.TryCreate(operation, UriKind.Absolute, out _) || operation.StartsWith("//", StringComparison.Ordinal))
            {
                throw new ArgumentException("Rules Engine operation must be a relative path.", nameof(operation));
            }

            return $"{baseUri.TrimEnd('/')}/{operation.TrimStart('/')}";
        }
    }
}
