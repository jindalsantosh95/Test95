using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
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
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class MemberDetailsQuery
    {
        private readonly ILogger<MemberDetailsQuery> _logger;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly static ConcurrentDictionary<string, string> msAuthToken = new();
        private readonly static ConcurrentDictionary<string, SemaphoreSlim> msAuthTokenLocks = new();
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IRefDataService<TierThresholdDetails> _tierThresholdService;
        private readonly IRefDataService<CountryRefData> _countriesRefService;
        private readonly ICustomerTransactionQueryProvider<CustomerProfileDTO> _customerTransCustomerProfile;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClubMember;

        public MemberDetailsQuery(ILogger<MemberDetailsQuery> logger, IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector, ITokenB2CProvider<string> tokenB2CProvider, IRefDataService<TierThresholdDetails> tierThresholdService, ICustomerTransactionQueryProvider<CustomerProfileDTO> customerTransCustomerProfile, ICustomerTransactionQueryProvider<FcMemberDetails> customerTransFlyingClubMember, IRefDataService<CountryRefData> countriesRefService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _tierThresholdService = tierThresholdService ?? throw new ArgumentNullException(nameof(tierThresholdService));
            _countriesRefService = countriesRefService;
            _customerTransCustomerProfile = customerTransCustomerProfile ?? throw new ArgumentNullException(nameof(customerTransCustomerProfile));
            _customerTransFlyingClubMember = customerTransFlyingClubMember ?? throw new ArgumentNullException(nameof(customerTransFlyingClubMember));
        }

        [GraphQLName("accountMemberDetails")]
        [GraphQLDescription("Receive Flying Club Member details")]
        public async Task<MemberDetailsResolverResponse> GetMemberDetails(int vouchersCount, int activitiesCount, int flightsCount)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                APIResponse<VRedResponse> vredResponse = null;
                APIResponse<CustomerTransactionDocument> customerTransactionResponse = null;
                APIResponse<LoyaltyMemberAccountDetailsResponse> loyaltyMemberAccountDetailsResponse = null;
                IEnumerable<TierThresholdDetails> tierThresholdDetails = null;
                IEnumerable<CountryRefData> countryDetails = null;

                var headersCustomerMS = GetHeader(Common.Constants.Environment.CustomerOcpKey);
                var headersCustomerTranMS = GetHeader(Common.Constants.Environment.CustTransOcpKey);
                var headersVRedMS = GetHeader(Common.Constants.Environment.VRedOcpSubID);
                var headersMemberManagementMS = GetHeader(Common.Constants.Environment.MemberManagementOcpKey);

                APIResponse<CustomerDocument> customerResponse = await _httpConnector.GetAsync<CustomerDocument>(ConfigReader.Get(Environment.CustomerMSURLForMemberDetails).Replace("{memberNumber}", claim?.FcNumber), headersCustomerMS, await GetToken(ConfigReader.Get(Environment.CustomerAppId), Application.CustomerMS));

                FcMemberDetails customerTransactionsFlyingClub = await _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Application.GetFcMemberDetails}", customerResponse.Response.CustomerId);

                var customerProfile = await _customerTransCustomerProfile.CustomerTransactionQueryCustomerProfile(ConfigReader.Get(Environment.CustTransactionMsUrl), customerResponse.Response.CustomerId);

                if (!customerTransactionsFlyingClub.FlyingClubMemberDocuments.First().TierCode.Is(Application.VirginAccountTierCode))
                    Parallel.Invoke(
                        new Action(() => tierThresholdDetails = _tierThresholdService.Query().GetAwaiter().GetResult()),
                        new Action(() => vredResponse = _httpConnector.GetAsync<VRedResponse>($"{ConfigReader.Get(Environment.VredMsUrl)}{Application.VredMSVirginPointsEndPoint}".Replace("{memberID}", claim?.FcNumber), headersVRedMS, GetToken(ConfigReader.Get(Environment.VRedAppId), Application.VRedMS).GetAwaiter().GetResult()).GetAwaiter().GetResult()),
                        new Action(() => countryDetails = _countriesRefService.Query().GetAwaiter().GetResult()),
                        new Action(() => customerTransactionResponse = _httpConnector.GetAsync<CustomerTransactionDocument>($"{ConfigReader.Get(Environment.CustTransactionMsUrl).TrimEnd('/')}/{customerResponse.Response.CustomerId}/tierDetails", headersCustomerTranMS, GetToken(ConfigReader.Get(Environment.CustTransAppId), Application.CustomerTransaction).GetAwaiter().GetResult()).GetAwaiter().GetResult()),
                        new Action(() => loyaltyMemberAccountDetailsResponse = _httpConnector.GetAsync<LoyaltyMemberAccountDetailsResponse>($"{ConfigReader.Get(Environment.LoyaltyMemberManagementMsUrl)}{Application.MemberAccountRetrievalEndPoint}".Replace("{custid}", customerResponse.Response.CustomerId), headersMemberManagementMS, GetToken(ConfigReader.Get(Environment.MemberManagementAppId), "MemberManagement").GetAwaiter().GetResult()).GetAwaiter().GetResult())
                         );

                var primaryAddress = customerResponse?.Response?.AddressDetails?.Where(x => x.primary == "Y");

                if (primaryAddress != null && primaryAddress.Any())
                {
                    primaryAddress.First().PreferredFlag = Application.PrimaryIndicator;
                    primaryAddress.First().CountryCode = countryDetails.First(x => x.CountryName.Is(primaryAddress?.First().Country))?.IATACountryCode;
                }

                else
                    primaryAddress = new List<AddressCustMS>()
                    { new()};

                var flyingClubMemberDetails = customerTransactionsFlyingClub.FlyingClubMemberDocuments.First().TierCode.Is(Application.VirginAccountTierCode) ?
                    new FlyingClubMemberDetails(customerResponse.Response, customerProfile, customerTransactionsFlyingClub) :
                    new FlyingClubMemberDetails(customerResponse.Response, customerTransactionResponse.Response, vredResponse.Response, tierThresholdDetails, primaryAddress, customerProfile, customerTransactionsFlyingClub);

                if (loyaltyMemberAccountDetailsResponse?.Response?.loyaltyMemberAccount is not null)
                {
                    flyingClubMemberDetails.ApplyRevisedVsTierPoints(
                        ParseOrDefault(loyaltyMemberAccountDetailsResponse.Response.loyaltyMemberAccount.RollingCoreTierPoints),
                        ParseOrDefault(loyaltyMemberAccountDetailsResponse.Response.loyaltyMemberAccount.RetainCoreTierPoints));
                }

                return new MemberDetailsResolverResponse()
                {
                    flyingClubMemberDetails = flyingClubMemberDetails,
                    ActivitiesCount = activitiesCount,
                    VouchersCount = vouchersCount,
                    FlightsCount = flightsCount
                };
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Login Token validation service");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Member Details Query  failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Member Details query\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberDetailsError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception in Member Details query");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberDetailsError)
                            .Build());
            }
        }

        private async Task<string> GetToken(string appId, string microService)
        {
            if (msAuthToken.TryGetValue(microService, out var cachedToken) && cachedToken.IsJwtTokenValid())
                return cachedToken;

            var tokenLock = msAuthTokenLocks.GetOrAdd(microService, _ => new SemaphoreSlim(1, 1));

            await tokenLock.WaitAsync();
            try
            {
                if (msAuthToken.TryGetValue(microService, out cachedToken) && cachedToken.IsJwtTokenValid())
                    return cachedToken;

                var token = await _tokenProvider.AcquireTokenAsync(appId);
                msAuthToken[microService] = token;

                return token;
            }
            finally
            {
                tokenLock.Release();
            }
        }

        private static int ParseOrDefault(string value)
        {
            return int.TryParse(value, out int parsedValue) ? parsedValue : 0;
        }

        private static Dictionary<string, string> GetHeader(string configKey)
        {
            return new Dictionary<string, string>
            {
                { Application.OcpKeyHeader, ConfigReader.Get(configKey) }
            };
        }
    }
}
