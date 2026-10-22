using System;
using System.Security.Cryptography;
using System.Text;

namespace VAA.EAPI.GraphQL.Common.Helper
{
    public static class Hash
    {
        public static string SHA256Managed(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            byte[] hashBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hashBytes);
        }
    }
}
