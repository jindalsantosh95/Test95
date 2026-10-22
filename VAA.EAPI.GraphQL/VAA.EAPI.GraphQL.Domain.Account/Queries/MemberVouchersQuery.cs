using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class MemberVouchersQuery
    {
        private readonly ILogger<MemberVouchersQuery> _logger;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;

        public MemberVouchersQuery(ILogger<MemberVouchersQuery> logger, ITokenB2CProvider<string> tokenB2CProvider, IAccountCustomerService customerProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [GraphQLName("memberVoucherDetails")]
        [GraphQLDescription("Receive Flying club member voucher details")]
        public async Task<VoucherDetails> GetMemberVoucherDetails([Service] IVoucherQueryProvider _voucherQueryProvider, int page, int pageSize)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse)
                     ?? throw new GraphQLException(ErrorBuilder.New()
                         .SetMessage("Miscellaneous technical error")
                         .SetCode(Constants.Application.VouchersError)
                         .Build());

                if (string.IsNullOrEmpty(claim.CustomerId))
                    claim.CustomerId = (await _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber))?.Customer?.CustomerId;

                if (string.IsNullOrEmpty(claim.CustomerId))
                {
                    throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage("Miscellaneous technical error")
                        .SetCode(Constants.Application.VouchersError)
                        .Build());
                }

                return await _voucherQueryProvider.GetAccountMemberVouchers(claim.CustomerId, page, pageSize);

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Member Vouchers Query failed");
                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New();
                    errorDetails.ForEach(error =>
                    {
                        builder.SetCode(error.Code);
                        builder.SetMessage(error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.VouchersError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in MemberVouchersQuery\r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.VouchersError)
                            .Build());
            }
        }
    }
}
