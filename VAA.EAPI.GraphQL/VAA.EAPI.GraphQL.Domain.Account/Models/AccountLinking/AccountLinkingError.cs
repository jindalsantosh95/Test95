using HotChocolate;
using VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking
{
    internal static class AccountLinkingError
    {
        internal static GraphQLException Create(string code, string message, int status, bool retryable = false) =>
            new(ErrorBuilder.New().SetMessage(message).SetCode(code)
                .SetExtension("httpStatusCode", status)
                .SetExtension("transportStatusCode", status)
                .SetExtension("retryable", retryable)
                .SetExtension("correlationId", AsyncContext.Current?.CorrelationId.ToString() ?? Guid.Empty.ToString())
                .Build());

        internal static GraphQLException InvalidTransaction(string message) =>
            Create("ACCOUNT_LINKING_TRANSACTION_INVALID", message, 400);
    }
}
