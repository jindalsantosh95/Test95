using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Infrastructure.Helper
{
    public static class SqlConnectionHelper
    {
        public const string _azureSqlAccessTokenScope = "https://database.windows.net/.default";
        private const string _authenticationKeyword = "Authentication";
        private static readonly ConcurrentDictionary<string, string> _managedIdentityTokens = new(StringComparer.OrdinalIgnoreCase);

        public static void ConfigureManagedIdentitySqlConnection(SqlConnection connection, string tokenScope = _azureSqlAccessTokenScope)
        {
            ArgumentNullException.ThrowIfNull(connection);
            ArgumentException.ThrowIfNullOrWhiteSpace(tokenScope);

            if (!string.IsNullOrWhiteSpace(connection.ConnectionString))
                connection.ConnectionString = RemoveSqlAuthentication(connection.ConnectionString);

            connection.AccessToken = GetManagedIdentityToken(tokenScope);
        }

        public static string RemoveSqlAuthentication(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            SqlConnectionStringBuilder builder = new(connectionString);
            builder.Remove(_authenticationKeyword);

            return builder.ConnectionString;
        }

        private static string GetManagedIdentityToken(string tokenScope)
        {
            if (_managedIdentityTokens.TryGetValue(tokenScope, out var cachedToken) && cachedToken.IsJwtTokenValid())
                return cachedToken;

            DefaultAzureCredentialOptions options = new() { ExcludeSharedTokenCacheCredential = true };
            DefaultAzureCredential credential = new(options);
            AccessToken token = credential.GetToken(new TokenRequestContext(
            [
                tokenScope
            ]));

            _managedIdentityTokens[tokenScope] = token.Token;

            return token.Token;
        }
    }
}
