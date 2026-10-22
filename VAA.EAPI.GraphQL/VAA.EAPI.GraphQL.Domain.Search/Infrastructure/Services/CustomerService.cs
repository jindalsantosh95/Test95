using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services
{
    public class CustomerService : ICustomerProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ILogger<CustomerService> _logger;


        public CustomerService(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ILogger<CustomerService> logger)
        {
            _tokenProvider = tokenProvider;
            _connector = connector;
            _logger = logger;
        }
        public async Task<CustomerDTO> GetCustomer(string customerId, string memberNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                    throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

                string customerUri = !string.IsNullOrEmpty(customerId)
                    ? customerUri = ConfigReader.Get(Common.Constants.Environment.CustomerUrlGetbyCustID).Replace("{customerid}", customerId)
                    : customerUri = ConfigReader.Get(Common.Constants.Environment.CustomerUrlGetByMemNum).Replace("{memberid}", memberNumber);

                // Get a token
                var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

                // Retrieve Customer
                Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) } };

                return (await _connector.GetAsync<CustomerDTO>(customerUri, headers, token)).Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex.Message}", ex);
                return null;
            }
        }
    }
}
