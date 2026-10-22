using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Helpers
{
    public class TokenHelper
    {
        public static string GetAuthorizationToken()
        {
            var bindingData = AsyncContext.Current?.Context?.BindingContext?.BindingData;
            if (bindingData is not null && bindingData.TryGetValue("Headers", out var headersObj))
            {
                if (headersObj is string headersStr)
                {
                    var headers = Json.Deserialize<Dictionary<string, string>>(headersStr);
                    headers.TryGetValue("Authorization", out var authorizationHeader);
                    var tokenParts = authorizationHeader?.Split(' ');

                    if (tokenParts?.Length >= 2)
                    {
                        return tokenParts[1];
                    }
                }
            }

            return string.Empty;
        }

        public static async Task<string> GetFQTNumber(ITokenB2CProvider<string> _tokenB2CProvider)
        {
            var token = GetAuthorizationToken();
            if (token != string.Empty)
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(token);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);
                return claim?.FcNumber;
            }
            return string.Empty;
        }
    }
}
