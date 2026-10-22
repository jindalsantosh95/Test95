using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class AddressHistoryQueryProvider : CustomerTransactionsProviderBase, IAddressHistoryQueryProvider
    {
        public AddressHistoryQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<IEnumerable<AddressHistory>> GetAddressHistory(string customerId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerAddressHistoryRetrievalEndPoint}".Replace("{customerId}", customerId);
            string getCustomerUri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerDocByCustomerIdEndPoint}".Replace("{customerId}", customerId);

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            // Retrieve AddressHistory
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };
            List<AddressHistory> addressHistories = (await _connector.GetAsync<List<AddressHistory>>(uri, headers, token)).Response;

            // Retrieve Customer Document
            var customer = (await _connector.GetAsync<Customers>(getCustomerUri, headers, token)).Response;

            if (customer != null && customer.AddressDetails.Any(x => x.Primary == "Y"))
                addressHistories.Add((AddressHistory)customer);

            return addressHistories;
        }
    }
}