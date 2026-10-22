using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class CustomerQueryProvider : ICustomerQueryProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public CustomerQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public async Task<Customers> GetCustomerDoc(string customerId)
        {
            string customerUri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerDocByCustomerIdEndPoint}".Replace("{customerId}", customerId);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            // Retrieve Customer
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };

            return (await _connector.GetAsync<Customers>(customerUri, headers, token)).Response;
        }

        public async Task<IEnumerable<CustomerSearch>> GetCustomerSearch(string phoneNumber)
        {
            string customerUri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerSearchByphoneEndPoint}".Replace("{phoneNumber}", phoneNumber);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            // Retrieve Customer Search
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };

            return (await _connector.GetAsync<IEnumerable<CustomerSearch>>(customerUri, headers, token)).Response;
        }
    }
}
