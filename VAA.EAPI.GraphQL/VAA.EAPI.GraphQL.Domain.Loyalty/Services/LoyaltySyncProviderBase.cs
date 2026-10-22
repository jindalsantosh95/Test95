using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class LoyaltySyncProviderBase
    {
        protected readonly IOAuthTokenProvider _tokenProvider;
        protected readonly IHttpConnector _connector;

        public LoyaltySyncProviderBase(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        protected async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.LoyaltySyncAppId));
        }

        protected Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.LoyaltySyncOcpSubId)}
            };
        }
    }
}
