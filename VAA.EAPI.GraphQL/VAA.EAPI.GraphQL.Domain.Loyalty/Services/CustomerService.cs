using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class CustomerService : ICustomerProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public CustomerService(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider;
            _connector = connector;
        }

        public async Task<CustomerDTO> GetCustomer(string customerId, string memberNumber)
        {
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

            string customerUri = !string.IsNullOrEmpty(customerId)
                ? $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerByCustomerIdEndPoint}".Replace("{customerid}", customerId)
                : $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerByMemberIdEndPoint}".Replace("{memberid}", memberNumber);

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            // Retrieve Customer
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };

            return (await _connector.GetAsync<CustomerDTO>(customerUri, headers, token)).Response;
        }
    }
}
