using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class TenureDetailsQuery
    {
        private readonly ILogger<TenureDetailsQuery> _logger;
        private readonly IMemberCustomAttributeProvider _memberCustomAttributeProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAccountCustomerService _customerProvider;
        protected readonly TimeProvider _timeProvider;

        public TenureDetailsQuery(ILogger<TenureDetailsQuery> logger, IMemberCustomAttributeProvider memberCustomAttributeProvider, ITokenB2CProvider<string> tokenB2CProvider, IAccountCustomerService customerProvider, TimeProvider timeProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memberCustomAttributeProvider = memberCustomAttributeProvider ?? throw new ArgumentNullException(nameof(memberCustomAttributeProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(_timeProvider));
        }

        [GraphQLName("accountTenureDetails")]
        [GraphQLDescription("Retrieve High Five Details for account")]
        public async Task<MemberCustomAttributesResponseAccount> AccountTenureDetails()
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

            var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

            if (string.IsNullOrEmpty(claim.CustomerId))
                claim.CustomerId = _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber).GetAwaiter().GetResult()?.Customer?.CustomerId;

            try
            {
                var result = new MemberCustomAttributesResponseAccount { MemberCustomAttributesDocuments = await _memberCustomAttributeProvider.GetMemberCustomerAttributeData(claim.CustomerId) };

                var currentYear = _timeProvider.GetUtcNow().Year.ToString();

                foreach (var item in result.MemberCustomAttributesDocuments)
                {
                    item.MemberLastFiveVSActivityYear = (item.MemberLastFiveVSActivityYear ?? Enumerable.Empty<MemberLastFiveVSActivityYearAccount>())
                        .OrderBy(y => int.Parse(y.ActivityYear))
                        .ToList();

                    item.IsAnActiveYear = (item.MemberLastFiveVSActivityYear ?? Enumerable.Empty<MemberLastFiveVSActivityYearAccount>())
                        .OrderByDescending(y => y.ActivityYear)
                        .FirstOrDefault() is { } last
                    && last.ActivityYear == currentYear
                    && !string.IsNullOrWhiteSpace(last.BenefitGiven)
                    && last.BenefitGiven.Is("true");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.Message, "TenureDetailsQuery", ((int)ex.StatusCode).ToString());
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Unhandled Exception in TenureDetailsQuery")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberCustomAttributesError)
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "TenureDetailsQuery", Constants.Application.MemberCustomAttributesError);
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberCustomAttributesError)
                            .Build());
            }
        }
    }
}
