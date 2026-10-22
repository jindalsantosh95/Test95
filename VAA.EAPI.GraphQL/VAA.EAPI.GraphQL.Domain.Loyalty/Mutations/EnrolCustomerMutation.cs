using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]

    public class EnrolCustomerMutation
    {
        protected readonly IOAuthTokenProvider _tokenProvider;
        protected readonly IHttpConnector _connector;
        protected readonly ILogger<EnrolCustomerMutation> _logger;

        public EnrolCustomerMutation(ILogger<EnrolCustomerMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        [GraphQLName("loyaltyEnrolCustomer")]
        [GraphQLDescription("Enrols a customer into loyalty programme.")]
        public async Task<string> EnrolCustomer(CreateCustomerRequest request)
        {
            try
            {
                if (ConfigReader.Get<bool>(Common.Constants.Environment.EnableCustomerMs))
                {
                    string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.CustomerBasicEnrolmentEndPoint}";
                    await _connector.SendAsync<EAPIResponse>(uri, GetCustomerHeaders(request.ChannelId), await GetCustomerMsToken(), HttpMethod.Post, Json.Serialize(request));
                    return "Ok";
                }
                else
                {
                    string uri = ConfigReader.Get(Common.Constants.Environment.BasicEnrolmentMSUrl);
                    await _connector.SendAsync<EAPIResponse>(uri, GetHeaders(request.ChannelId), await GetToken(), HttpMethod.Post, Json.Serialize(request));
                    return "Ok";
                }

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Unable to Enrol the request - {ex.Message}", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in VAA.EAPI.GraphQL.Domain.Loyalty.Mutations.EnrolCustomer() - {ex.Message}", ex);
                throw;
            }
        }

        private async Task<string> GetCustomerMsToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));
        }
        private async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.EnrolmentAPIAppId));
        }

        private static Dictionary<string, string> GetCustomerHeaders(string channelId)
        {
            return new Dictionary<string, string>
            {
                { "ChannelId",  channelId },
                {  Common.Constants.Application.OcpKeyHeader , ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
            };
        }
        private static Dictionary<string, string> GetHeaders(string channelId)
        {
            return new Dictionary<string, string>()
            {
                { "ChannelId", channelId },
                {  Common.Constants.Application.OcpKeyHeader , ConfigReader.Get(Common.Constants.Environment.EnrolmentMSOcpKey) }
            };
        }
    }
}
