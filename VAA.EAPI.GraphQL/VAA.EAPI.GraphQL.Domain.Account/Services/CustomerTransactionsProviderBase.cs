using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class CustomerTransactionsProviderBase
    {
        private readonly IOAuthTokenProvider _tokenProvider;

        protected readonly IHttpConnector _connector;

        public CustomerTransactionsProviderBase(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        protected async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.CustTransAppId));
        }

        protected Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                { Application.OcpKeyHeader, ConfigReader.Get(Environment.CustTransOcpKey)}
            };
        }
    }
}
