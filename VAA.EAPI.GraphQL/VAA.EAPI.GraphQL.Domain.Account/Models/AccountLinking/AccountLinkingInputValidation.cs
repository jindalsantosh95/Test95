using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking
{
    internal static class AccountLinkingInputValidation
    {
        internal static void Validate(string transactionId, string clientId, string resource)
        {
            if (!Valid(transactionId, 128) || !Valid(clientId, 256) || !Valid(resource, 2048))
                throw AccountLinkingError.InvalidTransaction("Valid transactionId, clientId and resource are required.");
        }

        private static bool Valid(string value, int maximum) =>
            !value.IsNullOrWhiteSpace() && value.Length <= maximum && !value.Any(char.IsControl);
    }
}
