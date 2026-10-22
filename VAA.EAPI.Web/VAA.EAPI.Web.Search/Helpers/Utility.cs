using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VAA.EAPI.Web.Search.Helpers
{
    public static class Utility
    {
        private static Regex fareIdRegex = new(@"(?'sliceId'slice[0-9]*\.[0-9])\|(?'offerId'.*)$", RegexOptions.Compiled);

        public static Match GetSliceIDOfferID(string fareId)
        {
            return fareIdRegex.Match(fareId);
        }

        public static string GetAuthorizationToken(HttpHeadersCollection reqHeaders, bool tokenValueWithBearer = true)
        {
            if (reqHeaders == null || !reqHeaders.TryGetValues("Authorization", out var authorizationHeader))
            {
                throw new KeyNotFoundException("Authorization header not found.");
            }

            if (tokenValueWithBearer)
            {
                return authorizationHeader.First();
            }

            else
            {
                var tokenParts = authorizationHeader.First().Split(' ');

                if (tokenParts.Length < 2)
                {
                    throw new FormatException("Invalid Authorization header format.");
                }

                return tokenParts.Last();
            }
        }


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
    }
}
