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
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;


namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{

    [ExtendObjectType(OperationTypeNames.Query)]

    public class PointsDotComQuery
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<PointsDotComQuery> _logger;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IRefDataService<CountryRefData> _countriesRefService;
        private readonly IHttpConnector _httpConnector;
        private readonly static Dictionary<string, string> msAuthToken = new();


        public PointsDotComQuery(ITokenB2CProvider<string> tokenB2CProvider, ILogger<PointsDotComQuery> logger, IRefDataService<CountryRefData> countriesRefService, IHttpConnector httpConnector, IOAuthTokenProvider tokenProvider)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _countriesRefService = countriesRefService ?? throw new ArgumentNullException(nameof(countriesRefService));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        [GraphQLName("accountPointDotComDetails")]
        [GraphQLDescription("Get SSO Token")]
        public async Task<string> GetPointsDotComDetails(string onBehalf)
        {
            if (ConfigReader.Get<bool>(Environment.EnableMockResponseForPointDotCom))
                return "613e62fd614aca2401daabe3f7ea048a57f41883c75fe5f0c8b06e5ee55f877645c363dc5a61991aec2576119a7d3809";

            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                if (onBehalf.TryParse(out ShopsAway res))
                {
                    var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                    var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                    var customer = _httpConnector.GetAsync<CustomerDocument>(ConfigReader.Get(Environment.CustomerMSURLForMemberDetails).Replace("{memberNumber}", claim?.FcNumber), GetHeaders(ConfigReader.Get(Environment.CustomerOcpKey)), GetToken(ConfigReader.Get(Environment.CustomerAppId), Application.CustomerMS).GetAwaiter().GetResult()).GetAwaiter().GetResult().Response;

                    APIResponse<CustomerTransactionDocument> customerTransactionResponse = null;
                    APIResponse<VRedResponse> vredResponse = null;

                    Parallel.Invoke(
                       new Action(() => customerTransactionResponse = _httpConnector.GetAsync<CustomerTransactionDocument>($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}/{customer.CustomerId}/tierDetails", GetHeaders(ConfigReader.Get(Environment.CustTransOcpKey)), GetToken(ConfigReader.Get(Common.Constants.Environment.CustTransAppId), Application.CustomerTransaction).GetAwaiter().GetResult()).GetAwaiter().GetResult()),
                       new Action(() => vredResponse = _httpConnector.GetAsync<VRedResponse>($"{ConfigReader.Get(Environment.VredMsUrl)}{Application.VredMSVirginPointsEndPoint}".Replace("{memberID}", claim?.FcNumber), GetHeaders(ConfigReader.Get(Environment.VRedOcpSubID)), GetToken(ConfigReader.Get(Environment.VRedAppId), Application.VRedMS).GetAwaiter().GetResult()).GetAwaiter().GetResult()));

                    var countryDetails = await _countriesRefService.Query();

                    string countrycode = countryDetails.FirstOrDefault(x => x.CountryName == customer.AddressDetails.FirstOrDefault(x => x.primary == "Y").Country).IATACountryCode;
                    string currencyCode = GetCurrencyCode(countrycode);
                    string membershipLevel = GetMembershipLevel(customerTransactionResponse.Response.TierCode);
                    string accountCreatedDate = customer.CreatedOn.ToString("dd/MM/yyyy");
                    string memberDetails = string.Empty;


                    if (res == ShopsAway.Buy || res == ShopsAway.Gift)
                        memberDetails = $"{customer.LoyaltyMemberNumber}|{customer.FirstName}|{customer.LastName}|{vredResponse.Response.Current}|{claim.EmailAddress}|{countrycode}|{accountCreatedDate}|{currencyCode}||{customer.LoyaltyMemberNumber}||{membershipLevel}";
                    else
                        memberDetails = $"{customer.LoyaltyMemberNumber}|{customer.FirstName}|{customer.LastName}|{vredResponse.Response.Current}|{claim.EmailAddress}|{countrycode}|{accountCreatedDate}|{currencyCode}||{customer.LoyaltyMemberNumber}|||{Constants.Application.DefaultLanguage}|||{membershipLevel}";

                    return (await _httpConnector.SendAsync<string>(ConfigReader.Get(Environment.SecurityMSURLForGetTokenForShopsAway), GetHeaders(ConfigReader.Get(Common.Constants.Environment.CommonEAPIOcpAPIMKey)), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.SecurityAppId)), HttpMethod.Post, Json.Serialize(new { memberDetails = memberDetails }))).Response;

                }
                else
                {
                    _logger.LogError("Points DotCom Details Query failed");
                    throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Incorrect Input parameter value \r\n")
                            .SetCode(Constants.Application.PointsDotComError)
                            .Build());
                }


            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Points DotCom Details Query failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Points DotCom Details Query failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Points DotCom Details Query \r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.PointsDotComError)
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Points DotCom Details Query \r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.PointsDotComError)
                            .Build());
            }
        }

        private static Dictionary<string, string> GetHeaders(string ocpSubId)
        {
            return new Dictionary<string, string>
            {
                {  Application.OcpKeyHeader , ocpSubId }
            };
        }
        private async Task<string> GetToken(string appId, string microService)
        {
            if (msAuthToken.ContainsKey(microService) && msAuthToken[microService].IsJwtTokenValid())
                return msAuthToken[microService];

            return msAuthToken[microService] = await _tokenProvider.AcquireTokenAsync(appId);
        }

        private string GetMembershipLevel(string tierCode)
        {
            return tierCode switch
            {
                Application.SilverTier => Constants.Application.MembershipLevelForSilver,
                Application.GoldTier => Constants.Application.MembershipLevelForGold,
                _ => Constants.Application.MembershipLevelForRed,
            };


        }
        private string GetCurrencyCode(string countryCode)
        {
            return countryCode.Is(Constants.Application.UsCountryCode) ? Constants.Application.UsCurrencyCode :
            Constants.Application.DefaultCurrencyCode;
        }
    }
}
