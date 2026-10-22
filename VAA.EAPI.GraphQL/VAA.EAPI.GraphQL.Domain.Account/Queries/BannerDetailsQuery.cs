using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Constants;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Banner;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class BannerDetailsQuery
    {
        private readonly ILogger<BannerDetailsQuery> _logger;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerTransactionQueryProvider<BannerDetails> _customerTransBannerDetail;

        public BannerDetailsQuery(ILogger<BannerDetailsQuery> logger, IAccountCustomerService customerProvider, ITokenB2CProvider<string> tokenB2CProvider, ICustomerTransactionQueryProvider<BannerDetails> customerTransBannerDetail)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerTransBannerDetail = customerTransBannerDetail ?? throw new ArgumentNullException(nameof(customerTransBannerDetail));
        }

        [GraphQLName("accountBannerDetails")]
        [GraphQLDescription("Retrieve Banner Details")]
        public async Task<BannerDetails> GetBannerDetailsQuery()
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
                    claim.CustomerId = _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber).GetAwaiter().GetResult()?.Customer?.CustomerId;
                }

                var custTransDoc = await _customerTransBannerDetail.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetBannerDetails}", claim.CustomerId);

                if (custTransDoc.Banners == null)
                {
                    return new BannerDetails
                    {
                        MemberNumber = claim.FcNumber,
                        Banners = Enum.GetValues<AccountBannerCode>().Select(bc => new Banners
                        {
                            BannerCode = bc.ToString(),
                            LastShownDateTime = null
                        })
                    };
                }

                return new BannerDetails
                {
                    MemberNumber = string.IsNullOrEmpty(custTransDoc.MemberNumber) ? null : custTransDoc.MemberNumber,
                    Banners = custTransDoc.Banners?.Where(b => Enum.IsDefined(typeof(AccountBannerCode), b.BannerCode))
                    .Select(b => new Banners
                    {
                        BannerCode = b.BannerCode,
                        LastShownDateTime = b.LastShownDateTime
                    })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled Exception in Retrieve Account Banner Details Query\r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Error while fetching Banner Details")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.BannerRetrievalErrorCode)
                            .Build());
            }
        }
    }
}
