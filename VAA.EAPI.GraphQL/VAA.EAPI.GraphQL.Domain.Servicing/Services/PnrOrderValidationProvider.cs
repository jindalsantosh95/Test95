using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Servicing.Interfaces;
using VAA.EAPI.GraphQL.Domain.Servicing.Models.PnrOrderValidation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Services
{
    public sealed class PnrOrderValidationProvider(IHttpConnector connector, IOAuthTokenProvider tokenProvider) : IPnrOrderValidationProvider
    {
        private readonly IHttpConnector _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        private readonly IOAuthTokenProvider _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

        public async Task<PnrOrderValidationResponse> ValidateAsync(PnrOrderValidationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Pnr.IsNullOrWhiteSpace())
                throw new ArgumentException("PNR is required.", nameof(request));

            var payload = Json.Serialize(new { Pnr = request.Pnr.Trim(), FirstName = request.GivenName?.Trim(), LastName = request.LastName?.Trim() });
            var response = await _connector.SendAsync<PnrOrderValidationResponse>($"{ConfigReader.Get(Environment.OrderDataMSUrl).TrimEnd('/')}/svc/orders/search", GetHeaders(), await GetTokenAsync(), HttpMethod.Post, payload);

            return response.Response;
        }

        private static Dictionary<string, string> GetHeaders() => new() { { Application.OcpKeyHeader, ConfigReader.Get(Environment.OrderDataOcpKey) } };

        private async Task<string> GetTokenAsync() => await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.OrderDataAppId));
    }
}
