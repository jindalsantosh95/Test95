using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using System.Net;
using System.Text.RegularExpressions;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.FlyingClubToken;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure
{
    [ExtendObjectType("Mutation")]
    public sealed class FlyingClubTokenMutation
    {
        private const string FlyingClubAccountNumberPattern = "^[0-9]{1,20}$";
        private readonly IFlyingClubTokenProvider _provider;

        public FlyingClubTokenMutation(IFlyingClubTokenProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        [Authorize(Roles = new[] { Constants.Application.FlyingClubApplicationTokenRole })]
        [GraphQLName("loyaltyIssueFlyingClubApplicationToken")]
        [GraphQLDescription("Issues a short-lived Identity broker token containing a Flying Club account-number claim.")]
        public async Task<FlyingClubTokenResponse> IssueAsync(FlyingClubTokenRequest request)
        {
            var accountNumber = request?.FlyingClubAccountNumber?.Trim();
            if (accountNumber is null || !RegexCompiled.IsMatch(accountNumber, FlyingClubAccountNumberPattern, RegexOptions.CultureInvariant))
            {
                throw new RequestValidationException("Flying Club account number must contain between 1 and 20 digits.");
            }

            request.FlyingClubAccountNumber = accountNumber;
            try
            {
                return await _provider.IssueAsync(request);
            }
            catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("The caller is not authorized to issue a Flying Club application token.")
                    .SetCode(exception.StatusCode.ToString())
                    .SetExtension("httpStatusCode", (int)exception.StatusCode.Value)
                    .Build());
            }
        }
    }
}
