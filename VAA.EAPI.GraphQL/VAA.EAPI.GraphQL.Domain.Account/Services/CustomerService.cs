using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class CustomerService : IAccountCustomerService
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
                ? customerUri = ConfigReader.Get(Common.Constants.Environment.CustomerUrlGetbyCustID).Replace("{customerid}", customerId)
                : customerUri = ConfigReader.Get(Common.Constants.Environment.CustomerUrlGetByMemNum).Replace("{memberid}", memberNumber);

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            // Retrieve Customer
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };

            return (await _connector.GetAsync<CustomerDTO>(customerUri, headers, token)).Response;
        }

        public async Task<List<CustomerProfileDetails>> GetCustomerDetailsByEmail(CustomerProfileRequest request)
        {

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            var customerProfileByEmailUrl = $"{ConfigReader.Try(Common.Constants.Environment.CustomerMsUrl)}/customerProfileSearch";
            Dictionary<string, string> headers = new()
                {
                    { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                };

            var httpResponse = await _connector.SendAsync(customerProfileByEmailUrl, headers, token, HttpMethod.Post, Json.Serialize(request));
            var responseJson = await httpResponse.Content.ReadAsStringAsync();
            var response = responseJson.IsNullOrEmpty() ? null : Json.Deserialize<List<CustomerProfileDetails>>(responseJson);

            if (response == null || !response.Any())
                return null;

            return response;
        }
    }
}
