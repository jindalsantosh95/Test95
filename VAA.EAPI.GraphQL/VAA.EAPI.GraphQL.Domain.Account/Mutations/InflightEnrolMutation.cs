using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]

    public class InflightEnrolMutation
    {
        protected readonly IOAuthTokenProvider _tokenProvider;
        protected readonly IHttpConnector _connector;
        protected readonly ILogger<InflightEnrolMutation> _logger;

        public InflightEnrolMutation(ILogger<InflightEnrolMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        [GraphQLName("inflightEnrolCustomer")]
        [GraphQLDescription("Enrols a customer into loyalty programme.")]
        public async Task<string> EnrolCustomer(InflightCustomerAccountRequest request)
        {
            try
            {
                if (ConfigReader.Try(Environment.EnrolmentTokenFlag, false))
                {
                    if (request.EnrolmentToken.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.EnrolmentToken));
                    request.CustomerDetails.ContactDetails.EmailDetails.First().EmailId = (await _connector.SendAsync<string>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.OtpTokenValidation}", Helper.GetHeadersForIdentityMs(request.ChannelId), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize(new { Token = request.EnrolmentToken }))).Response;
                }

                string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Application.CustomerBasicEnrolmentEndPoint}";
                await _connector.SendAsync<EAPIResponse>(uri, Helper.GetHeaders(request.ChannelId), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.CustomerAppId)), HttpMethod.Post, Json.Serialize(request));
                return "Ok";
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
    }
}
