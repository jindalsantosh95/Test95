using Microsoft.Extensions.Logging;
using System.Net.Http;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment;
using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class CustomerServiceProvider : ICustomerProvider, ICustomerServiceProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ILogger<CustomerServiceProvider> _logger;

        public CustomerServiceProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ILogger<CustomerServiceProvider> logger)
        {
            _tokenProvider = tokenProvider;
            _connector = connector;
            _logger = logger;
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

        public async Task<CustomerEnrolmentResponse> EnrolVAAccount(CustomerEnrolmentRequest enrolmentRequest)
        {
            if (enrolmentRequest == null)
                throw new ArgumentException("EnrolmentRequest can not have null value");

            string vaenrolmentUri = ConfigReader.Try(Common.Constants.Environment.CustomerMsUrl).TrimEnd('/') + Common.Constants.Application.VAAccountEnrolmentEndpoint;

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            // Retrieve Customer ocp key
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };

            return (await _connector.SendAsync<CustomerEnrolmentResponse>(vaenrolmentUri, headers, token, HttpMethod.Post, Json.Serialize(enrolmentRequest))).Response;
        }

        public async Task<List<CustomerProfileDetails>> GetCustomerDetailsByEmail(CustomerProfileRequest request)
        {
            try
            {
                var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

                var customerProfileByEmailUrl = $"{ConfigReader.Try(Common.Constants.Environment.CustomerMsUrl)}/customerProfileSearch";
                Dictionary<string, string> headers = new()
                {
                    { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                };

                var httpResponse = (await _connector.SendAsync(customerProfileByEmailUrl, headers, token, HttpMethod.Post, Json.Serialize(request)));
                var response = await httpResponse.Content.ReadAsAsync<List<CustomerProfileDetails>>();
                
                if (response == null || !response.Any())
                {
                    _logger.LogError($" Correlation Id - {AsyncContext.Current.CorrelationId} : : AutoMatch Failed");
                    return null;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $" Correlation Id - {AsyncContext.Current.CorrelationId} : : AutoMatch Failed");
                return null;
            }
        }
    }
}
