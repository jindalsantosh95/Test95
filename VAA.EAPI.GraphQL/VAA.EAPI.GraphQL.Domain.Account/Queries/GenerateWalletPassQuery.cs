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
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class GenerateWalletPassQuery
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly static Dictionary<string, string> msAuthToken = new();
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<GenerateWalletPassQuery> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClubMember;

        public GenerateWalletPassQuery(IAccountCustomerService customerProvider, ICustomerTransactionQueryProvider<FcMemberDetails> customerTransFlyingClubMember, ITokenB2CProvider<string> tokenB2CProvider, ILogger<GenerateWalletPassQuery> logger, IHttpConnector httpConnector, IOAuthTokenProvider tokenProvider)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerTransFlyingClubMember = customerTransFlyingClubMember ?? throw new ArgumentNullException(nameof(customerTransFlyingClubMember));
        }


        [GraphQLName("accountGenerateWalletPass")]
        [GraphQLDescription("Retrieve membership card details like Member Name, FCNumber, TierCode to add to the wallet")]
        public async Task<byte[]> GenerateWalletPass()
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                var customerResponse = await _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber);

                var customerFcmDetails = await _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Application.GetFcMemberDetails}", customerResponse.Customer.CustomerId);

                string memberName = $"{customerResponse.Customer.FirstName} {customerResponse.Customer.LastName}";

                return (await _httpConnector.SendAsync<byte[]>($"{ConfigReader.Get(Environment.WalletGeneratePassURL)}{Application.GeneratePassUrl}", GetHeaders(ConfigReader.Get(Common.Constants.Environment.CommonEAPIOcpAPIMKey)), await GetToken(ConfigReader.Get(Environment.WalletAppId), Application.WalletMS), HttpMethod.Post, Json.Serialize(new GeneratePassRequest
                {
                    FcNumber = claim?.FcNumber,
                    MemberName = $"{customerResponse.Customer.FirstName} {customerResponse.Customer.LastName}",
                    TierCode = customerFcmDetails.FlyingClubMemberDocuments.First().TierCode,
                    ValidUntil = customerFcmDetails.FlyingClubMemberDocuments.First().memberShipCardDetails.ExpiryDate
                }))).Response;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Wallet Generate Pass Query failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);
                {
                    var builder = ErrorBuilder.New().SetMessage("Wallet Generate Pass Query failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Wallet Generate Pass Query failed\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.GenerateWalletPassError)
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Wallet Generate Pass Query failed\r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.GenerateWalletPassError)
                            .Build());
            }
        }

        private async Task<string> GetToken(string appId, string microService)
        {
            if (msAuthToken.ContainsKey(microService) && msAuthToken[microService].IsJwtTokenValid())
                return msAuthToken[microService];

            return msAuthToken[microService] = await _tokenProvider.AcquireTokenAsync(appId);
        }
        private static Dictionary<string, string> GetHeaders(string ocpSubId)
        {
            return new Dictionary<string, string>
            {
                {  Application.OcpKeyHeader , ocpSubId }
            };
        }
    }
}
