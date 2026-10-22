using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services
{
    public class CustomerTransactionService : ICustomerTransactionProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public CustomerTransactionService(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<VoucherDetail>> RetrieveVoucher(string customerId)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).TrimEnd('/');

            return (await _connector.GetAsync<IEnumerable<VoucherDetail>>($"{uri}/{customerId}/vouchers", GetHeaders(), await GetToken())).Response;
        }

        private async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustTransAppId));
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustTransOcpKey) }
            };
        }
    }
}
