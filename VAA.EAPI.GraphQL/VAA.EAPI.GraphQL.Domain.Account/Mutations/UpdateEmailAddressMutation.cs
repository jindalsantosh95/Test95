using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class UpdateEmailAddressMutation
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IHttpConnector _connector;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly ILogger<UpdateEmailAddressMutation> _logger;
        private readonly IRefDataService<BlackListedEmails> _blackListedEmails;
        private readonly static Dictionary<string, string> msAuthToken = new();

        private readonly static Dictionary<string, string> headersIdentityMS = new Dictionary<string, string>()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.IdentityMSOcpKey)}
        };

        private readonly static Dictionary<string, string> headersCustomerMS = new Dictionary<string, string>()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey)}
        };

        public UpdateEmailAddressMutation(ITokenB2CProvider<string> tokenB2CProvider, ILogger<UpdateEmailAddressMutation> logger, IHttpConnector connector, IRefDataService<BlackListedEmails> blackListedEmails, IOAuthTokenProvider tokenProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _blackListedEmails = blackListedEmails ?? throw new ArgumentNullException(nameof(blackListedEmails));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        [GraphQLName("accountUpdateEmailAddress")]
        [GraphQLDescription("Update Customer's Email Address through Manage Account")]
        public async Task<string> UpdateEmail(UpdateEmailRequest newEmailAddress)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                var blacklistedEmails = Json.Deserialize<List<string>>(_blackListedEmails.Query().Result.First().Emails);

                if (blacklistedEmails.Any(x => x.Equals(newEmailAddress.EmailAddress, StringComparison.OrdinalIgnoreCase)))
                {
                    var builder = ErrorBuilder.New().SetMessage("Email BlackListed");
                    builder.SetExtension("code", Application.BlackListedEmaildErrorCode);
                    builder.SetExtension("message", Application.BlackListedEmaildErrorMessage);
                    builder.SetExtension("source", Application.BlackListedEmaildErrordErrorsource);
                    throw new GraphQLException(builder.Build());
                }

                var verifyMemberNewEmail = await _connector.SendAsync<string>(ConfigReader.Get(Common.Constants.Environment.IdentityMSURLForVerifyMemberInAzureB2C), headersIdentityMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize(newEmailAddress));

                if (!verifyMemberNewEmail.ResponseMessage.IsSuccessStatusCode)
                    return "Email Update Failed";

                var updateEmail = await _connector.SendAsync<string>(ConfigReader.Get(Common.Constants.Environment.MyAccountCustomerMSURLForEmailUpdate).Replace("{memberNumber}", claim?.FcNumber), headersCustomerMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId)), HttpMethod.Patch, Json.Serialize(new UpdateDBRequest
                {
                    NewEmailAddress = newEmailAddress.EmailAddress
                }));

                return updateEmail.Response;
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Email Update Failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Email Update failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Email Update Mutation\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.EmailUpdateError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Email Update Mutation\r\n{ex.Message}");
                throw;
            }
        }
    }
}
