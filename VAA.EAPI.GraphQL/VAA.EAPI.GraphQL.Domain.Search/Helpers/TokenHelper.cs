using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    public class TokenHelper
    {
        public static string GetAuthorizationToken()
        {

            if (AsyncContext.Current.Context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
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
    }
}
