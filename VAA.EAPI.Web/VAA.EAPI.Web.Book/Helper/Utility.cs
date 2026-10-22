using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Web.Common.Extensions;

namespace VAA.EAPI.Web.Book.Helper
{
    public class Utility
    {
        public static string TryGetAuthorizationToken(HttpHeadersCollection reqHeaders)
        {
            if (reqHeaders?.TryGetValues("Authorization", out var authorizationHeader) ?? false)
            {
                var tokenParts = authorizationHeader?.FirstOrDefault()?.Split(' ');

                if (tokenParts?.Length >= 2)
                {
                    return tokenParts[1];
                }
            }

            return string.Empty;
        }

        public static Dictionary<string, string> GetHeaders(string token)
        {
            var headers = new Dictionary<string, string>();

            if (!token.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {token}");
            }

            return headers;
        }
    }
}