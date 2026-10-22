using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Mappers.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;


namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]

    public class UpdateMarketingPreferenceMutation
    {
        private readonly ILogger<UpdateMarketingPreferenceMutation> _logger;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly static Dictionary<string, string> msAuthToken = new();

        private readonly static Dictionary<string, string> headersCustomerMS = new Dictionary<string, string>()
        {
            {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Environment.CustomerOcpKey)}
        };

        public UpdateMarketingPreferenceMutation(ILogger<UpdateMarketingPreferenceMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector, ITokenB2CProvider<string> tokenB2CProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
        }


        [GraphQLName("updateMarketingPreference")]
        [GraphQLDescription("Update Marketing Preference in Manage Account.")]
        public async Task<bool> MarketingPreference(UpdateMarketingPreferenceRequest updateMarketingPreferenceRequest)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                var customerResponse = _connector.GetAsync<CustomerDocument>(ConfigReader.Get(Common.Constants.Environment.CustomerMSURLForMemberDetails).Replace("{memberNumber}", claim?.FcNumber), headersCustomerMS, GetToken(ConfigReader.Get(Common.Constants.Environment.CustomerAppId), VAA.EAPI.GraphQL.Common.Constants.Application.CustomerMS).GetAwaiter().GetResult()).GetAwaiter().GetResult();

                var response = await _connector.SendAsync<UpdateMarketingPreferenceResponse>($"{ConfigReader.Get(Environment.CustTransactionMsUrl).TrimEnd('/')}/UpdateMarketingPreferences", Helper.GetHeaders(Common.Constants.Application.WebMyaChannelID), await GetToken(ConfigReader.Get(Environment.CustTransAppId), string.Empty), HttpMethod.Patch, Json.Serialize(UpdateMarketingPreferenceRequestMapper.GetUpdateMarketingPreference(claim, updateMarketingPreferenceRequest, customerResponse.Response)));

                if (response.Response.ErrorDetails != null && response.Response.ErrorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Update Marketing Preference failed");
                    response.Response.ErrorDetails.ToList().ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                return response.Response.PreferenceValue;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Update Marketing Preference Mutation Failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Marketing Preference Update failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while updating Marketing Preference for Member \r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberUpdatePreferenceError)
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Update Marketing Preference Mutation Failed \r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while updating Marketing Preference for Member")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberUpdatePreferenceError)
                            .Build());
            }


        }

        private async Task<string> GetToken(string appId, string microService)
        {
            if (msAuthToken.ContainsKey(microService) && msAuthToken[microService].IsJwtTokenValid())
                return msAuthToken[microService];

            return msAuthToken[microService] = await _tokenProvider.AcquireTokenAsync(appId);
        }
    }
}

