using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class PasswordResetConfirmationMutation
    {

        private readonly ILogger<PasswordResetConfirmationMutation> _logger;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ICustomerMutationProvider<TierDetail> _customerMutationProvider;

        public PasswordResetConfirmationMutation(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ILogger<PasswordResetConfirmationMutation> logger, ICustomerMutationProvider<TierDetail> customerMutationProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _customerMutationProvider = customerMutationProvider ?? throw new ArgumentNullException(nameof(customerMutationProvider));
        }

        [GraphQLName("accountEmailConfirmation")]
        [GraphQLDescription("Send Email for password reset")]
        public async Task<bool> confirmationEmail()
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                string emailAddress = TokenHelper.GetEmailFromToken(jwtPrincipalFeature.AccessToken.Token);

                if (string.IsNullOrEmpty(emailAddress))
                {
                    throw new InvalidOperationException("Email address not found in the token.");
                }

                var tierResponse = _customerMutationProvider.GetTierResponse(emailAddress).Result.Tier;

                var emailRequest = new SendEmailRequest
                {
                    Template = ConfigReader.Get(Environment.MyAccountPasswordResetConfirmationTemplate),
                    To = emailAddress
                };

                if (!string.IsNullOrEmpty(tierResponse))
                    emailRequest.TierCode = tierResponse;

                return (await _connector.SendAsync<bool>(ConfigReader.Get(Environment.MyAccountEmailMSURLForPasswordResetConfirmation), GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.EmailMSAppId)), HttpMethod.Post, Json.Serialize(emailRequest))).Response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in VAA.EAPI.GraphQL.Domain.Account.Mutations.PasswordResetConfirmationMutation() - {ex.Message}", ex);
                throw;
            }

        }
        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {  Application.OcpKeyHeader , ConfigReader.Get(Environment.CommonEAPIOcpAPIMKey) }
            };
        }
    }
}
