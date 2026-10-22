using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class EmailMutation
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ILogger<EmailMutation> _logger;

        public EmailMutation(ILogger<EmailMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        [GraphQLName("accountEmailVerification")]
        [GraphQLDescription("Verify a Email into loyalty programme.")]
        public async Task<bool> VerifyEmail(string activationToken)
        {
            try
            {
                if (!TokenHelper.CheckValidIssuer(activationToken) || TokenHelper.HasTokenExpired(activationToken))
                {
                    var builder = ErrorBuilder.New().SetMessage(Common.Constants.Application.VerificationTokenNotValidErrorMessage);
                    builder.SetExtension("code", Common.Constants.Application.VerificationTokenNotValidErrorCode);
                    builder.SetExtension("message", Common.Constants.Application.VerificationTokenNotValidErrorMessage);
                    builder.SetExtension("source", Common.Constants.Application.ErrorSource);
                    throw new GraphQLException(builder.Build());
                }

                string uri = ConfigReader.Get(Environment.MyAccountIdentityMSURLForVerifyEmailtoAzureB2C);
                string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.EnrolmentSignUpAppId));

                var headers = new Dictionary<string, string>
                {
                    { Application.ChannelId, Application.WebMyaChannelID },
                    { Application.OcpKeyHeader, ConfigReader.Get(Environment.IdentityOcpKey) }
                };

                var activationTokenClaim = TokenHelper.GetClaims(activationToken);

                await _connector.SendAsync<string>(uri, headers, token, HttpMethod.Post, Json.Serialize(new
                {
                    CustomerDetails = new
                    {
                        FirstName = activationTokenClaim?.FirstName,
                        LastName = activationTokenClaim?.LastName,
                        FcNumber = activationTokenClaim?.FcNumber,
                        ContactDetails = new
                        {
                            EmailDetails = new[]
                            {
                                    new {EmailId = activationTokenClaim?.Email }
                                }
                        },
                        AddressDetails = new[]
                        {
                            new {CountryCode = activationTokenClaim?.CountryCode }
                        }
                    },
                    ObjectId = activationTokenClaim?.ObjectId
                }));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in VAA.EAPI.GraphQL.Domain.Loyalty.Mutations.EmailVerification() - {ex.Message}", ex);
                throw;
            }
        }
    }
}
