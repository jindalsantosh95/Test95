using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{

    [ExtendObjectType("Mutation")]
    public class ManageBannerBannerMutation
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IHttpConnector _connector;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly ILogger<ManageBannerBannerMutation> _logger;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerMutationProvider<string> _customerMutationProvider;

        private readonly static Dictionary<string, string> headersIdentityMS = new Dictionary<string, string>()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.IdentityMSOcpKey)}
        };
        private readonly static Dictionary<string, string> headersTransactionMS = new Dictionary<string, string>()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustTransOcpKey)}
        };

        public ManageBannerBannerMutation(ITokenB2CProvider<string> tokenB2CProvider, IAccountCustomerService customerProvider, ILogger<ManageBannerBannerMutation> logger, IHttpConnector connector, IOAuthTokenProvider tokenProvider, ICustomerMutationProvider<string> customerMutationProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));

            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerMutationProvider = customerMutationProvider ?? throw new ArgumentNullException(nameof(customerMutationProvider)); ;
        }

        [GraphQLName("ManageBannerDetails")]
        [GraphQLDescription("Manage Banner details through Manage Account")]
        public async Task<string> ManageBannerBannerDetailsAsync(string bannerCode)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                if (string.IsNullOrEmpty(claim.CustomerId))
                {
                    claim.CustomerId = (await _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber))?.Customer?.CustomerId;
                }

                string bannerUrl = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl);
                string transToken = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustTransAppId));

                string getUurl = $"{bannerUrl}{claim.CustomerId}{Application.GetbannerDetails}";
                var bannerDetails = await _connector.SendAsync<BannerData>(getUurl, headersTransactionMS, transToken, HttpMethod.Get, string.Empty);

                if (!(bannerDetails.Response.Banners?.Any() ?? false) || !bannerDetails.Response.Banners.Any(b => b.BannerCode == bannerCode))
                {
                    string createUurl = $"{bannerUrl}{Common.Constants.Application.CreatebannerDetails}";
                    await _connector.SendAsync<object>(createUurl, headersTransactionMS, transToken, HttpMethod.Post, Json.Serialize(BannerMapper.CreateBanerResponseMapper(claim.CustomerId, claim.FcNumber, bannerCode)));
                }
                else
                {
                    string updateUrl = $"{bannerUrl}{claim.CustomerId}{Application.UpdatebannerDetails}";
                    await _connector.SendAsync<object>(updateUrl, headersTransactionMS, transToken, HttpMethod.Patch, Json.Serialize(BannerMapper.UpdateBanerResponseMapper(bannerCode)));
                }

                return "Banner Operation Successfully Completed";
            }
            catch (HttpRequestException ex)
            {
                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);
                _logger.LogError(ex, apiResponse);

                if (!errorDetails.IsNullOrEmpty())
                {
                    var builder = ErrorBuilder.New().SetMessage("Manage Banner Operation failed");
                    foreach (var error in errorDetails)
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    }

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Manage Banner Mutation\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.EmailUpdateError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Manage banner Mutation\r\n{ex.Message}");
                throw;
            }
        }
    }
}
