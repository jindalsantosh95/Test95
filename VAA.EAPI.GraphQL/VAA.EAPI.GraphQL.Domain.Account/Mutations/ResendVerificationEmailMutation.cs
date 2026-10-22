using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class ResendVerificationEmailMutation
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ILogger<ResendVerificationEmailMutation> _logger;

        public ResendVerificationEmailMutation(ILogger<ResendVerificationEmailMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        [GraphQLName("accountResendVerificationEmail")]
        [GraphQLDescription("Resend Verification Email Request and Extend Expiration Time for activation")]
        public async Task<string> VerificationEmail(EnrolToAzureB2CCommandRequest request)
        {
            try
            {
                _logger.LogInformation("Resending Verification Email Request");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                await _connector.SendAsync<string>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.Register}", GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.EnrolmentSignUpAppId)), HttpMethod.Post, Json.Serialize(request));

                return "Verification Email Request Sent Successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ResendVerificationEmail");
                throw;
            }
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                { "ChannelId", Environment.EnrolmentChannelId },
                {  Application.OcpKeyHeader , ConfigReader.Get(Environment.EnrolmentMSOcpKey) }
            };
        }
    }
}
