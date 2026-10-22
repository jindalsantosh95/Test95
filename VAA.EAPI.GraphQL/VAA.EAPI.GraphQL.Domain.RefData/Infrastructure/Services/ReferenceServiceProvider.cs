using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Services
{
    public sealed class ReferenceServiceProvider : IRefServiceProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public ReferenceServiceProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<CountryRefData>> RetrieveCountriesAsync()
        {
            string url = ConfigReader.Get(Constants.Environment.ReferenceMSCountriesUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.ReferenceMSAppId));

            return (await _connector.GetAsync<IEnumerable<CountryRefData>>(url, GetHeaders(), token)).Response;
        }

        public async Task<IEnumerable<FQTProgrammeRefData>> RetrieveFQTProgrammesAsync()
        {
            string url = ConfigReader.Get(Constants.Environment.ReferenceMSFQTUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.ReferenceMSAppId));

            return (await _connector.GetAsync<IEnumerable<FQTProgrammeRefData>>(url, GetHeaders(), token)).Response;
        }

        public async Task<VAA.EAPI.GraphQL.Domain.RefData.Models.MobileFeatures> RetrieveMobileFeaturesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.ReferenceMSAppId))
                .WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, ConfigReader.Get(Constants.Environment.ReferenceMSMobileFeaturesUrl));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
                foreach (var header in GetHeaders())
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                using var response = await _connector.SendAsync(request, cancellationToken, noWait: false);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return Common.Serialisation.Json.Deserialize<VAA.EAPI.GraphQL.Domain.RefData.Models.MobileFeatures>(json)
                    ?? throw new InvalidOperationException("Invalid mobile feature configuration response.");
            }
            catch (System.Net.Http.HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Missing server configuration is a technical failure, not a missing mobile resource.
                throw new HotChocolate.GraphQLException(HotChocolate.ErrorBuilder.New()
                    .SetMessage("Mobile feature configuration is unavailable.")
                    .SetCode("500")
                    .Build());
            }
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ReferenceMSOcpKey) },
            };
        }
    }
}
